﻿using Buddy.Coroutines;
using Clio.XmlEngine;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using GreyMagic;
using System;
using System.Linq;
using TreeSharp;
using Action = TreeSharp.Action;

namespace ff14bot.NeoProfiles.Tags
{
    [XmlElement("SelfRepair")]
    public class SelfRepairTag : ProfileBehavior
    {

        [XmlAttribute("Threshhold")]
        public float Threshhold { get; set; }

        [XmlAttribute("Void")]
        public bool Void { get; set; }

#if RB_CN
		public static int AgentId = 36;
#else
        public static int AgentId = 36;
#endif

        private bool _done = false;

        public override bool IsDone => _done;

        public new Composite Behavior()
        {
            return new PrioritySelector(
                // can tag execute?
                new Decorator(ret => !CanRepair(),
                    new Action(r => _done = true)
                ),
                new Decorator(ret => CanRepair(),
                    new PrioritySelector(
                        new Decorator(ret => !Repair.IsOpen,
                            new ActionRunCoroutine(async r =>
                            {
                                if (Void == true)
                                {
                                    OpenRepair();
                                }
                                else
                                {
                                    ActionManager.ToggleRepairWindow();
                                    await Coroutine.Wait(3000, () => Repair.IsOpen);
                                }
                            })
                        ),
                        new Decorator(ret => Repair.IsOpen,
                            new ActionRunCoroutine(async r =>
                            {
                                Repair.RepairAll();
                                if (await Coroutine.Wait(4000, () => SelectYesno.IsOpen))
                                {
                                    SelectYesno.ClickYes();
                                }
                                Repair.Close();
                                await Coroutine.Wait(3000, () => !Repair.IsOpen);
                            })
                        )
                    )
                )
            );
        }

        public bool CanRepair()
        {
            if (Threshhold <= 0 || Threshhold > 100)
            {
                Threshhold = 100f;
            }

            return InventoryManager.EquippedItems.Where(r => r.IsFilled && r.Condition < Threshhold).Count() > 0;
        }

        public static void OpenRepair()
        {
            PatternFinder patternFinder = new PatternFinder(Core.Memory);
            IntPtr off = patternFinder.Find("4C 8D 0D ? ? ? ? 45 33 C0 33 D2 48 8B C8 E8 ? ? ? ? Add 3 TraceRelative");
            IntPtr func = patternFinder.Find("48 89 5C 24 ? 57 48 83 EC ? 88 51 ? 49 8B F9");

            Core.Memory.CallInjected64<IntPtr>(func, AgentModule.GetAgentInterfaceById(AgentId).Pointer, 0, 0, off);
        }

        protected override void OnResetCachedDone()
        {
            _done = false;
        }

        protected override void OnDone()
        {
            TreeHooks.Instance.RemoveHook("TreeStart", _cache);
        }

        private Composite _cache;

        protected override void OnStart()
        {
            _cache = Behavior();
            TreeHooks.Instance.InsertHook("TreeStart", 0, _cache);
        }

    }
}