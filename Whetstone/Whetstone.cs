#pragma warning disable CA1416

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Eco.Gameplay.Components;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Skills;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Serialization;

namespace Whetstone
{
    [Serialized]
    [LocDisplayName("Tool Breaker")]
    [Category("Hidden")]
    [Weight(1)]
    [MaxStackSize(1)]
    public class ToolBreakerItem : Item
    {
        public override LocString DisplayDescription => Localizer.DoStr("Break selected tool immediately");

        public override void OnUsed(Player player, ItemStack itemStack)
        {
            base.OnUsed(player, itemStack);
            var selectedItem = player.User.Inventory.Toolbar.SelectedItem;
            if (selectedItem is not ToolItem tool) return;
            tool.Durability = 0;
        }
    }

    public class Whetstone : Item
    {
        protected virtual Tag RepairTag => null;
        protected virtual Type RepairItem => null;

        public override void OnUsed(Player player, ItemStack itemStack)
        {
            base.OnUsed(player, itemStack);
            var selectedItem = player.User.Inventory.Toolbar.SelectedItem;
            if (selectedItem is not ToolItem tool || !CanRepair(tool))
            {
                player.Error(Localizer.DoStr($"{DisplayName} cannot fix {selectedItem.DisplayName}!"));
                return;
            }

            if (!NeedRepair(tool))
            {
                player.Error(Localizer.DoStr($"{selectedItem.DisplayName} is all good"));
                return;
            }
            itemStack.TryModifyStack(player.User, -1, null, () =>
            {
                tool.Durability = 100;
                player.InfoBox(Localizer.DoStr($"{selectedItem.DisplayName} repaired to full durability!"));
            });
        }
        
        private bool CanRepair(ToolItem tool)
        {
            return CanRepairByItem(tool) || CanRepairByTag(tool);
        }

        private bool CanRepairByItem(ToolItem tool)
        {
            return RepairItem != null && tool.RepairItem.GetType() == RepairItem;
        }

        private bool CanRepairByTag(ToolItem tool)
        {
            return RepairTag != null && tool.RepairTag == RepairTag;
        }

        private static bool NeedRepair(DurabilityItem item)
        {
            return item.Durability < 100;
        }
    }

    [Serialized]
    [LocDisplayName("Wood Whetstone")]
    [Weight(800)]
    [MaxStackSize(50)]
    public class WoodWhetstoneItem : Whetstone
    {
        protected override Tag RepairTag => Tag.Wood;
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair wooden tools");
    }

    [Serialized]
    [LocDisplayName("Stone Whetstone")]
    [Weight(1000)]
    [MaxStackSize(50)]
    public class StoneWhetstoneItem : Whetstone
    {
        protected override Tag RepairTag => TagManager.Tag("Rock");
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair stone tools");
    }

    [Serialized]
    [LocDisplayName("Iron Whetstone")]
    [Weight(1000)]
    [MaxStackSize(50)]
    public class IronWhetstoneItem : Whetstone
    {
        protected override Type RepairItem => typeof(IronBarItem);

        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair iron tools");
    }

    [Serialized]
    [LocDisplayName("Steel Whetstone")]
    [Weight(1000)]
    [MaxStackSize(50)]
    public class SteelWhetstoneItem : Whetstone
    {
        protected override Type RepairItem => typeof(SteelBarItem);
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair modern/steel tools");
    }

    [RequiresSkill(typeof(BasicEngineeringSkill), 0)]
    public class StoneWhetstoneRecipe : RecipeFamily
    {
        public StoneWhetstoneRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                "Stone Whetstone",
                Localizer.DoStr("Stone Whetstone"),
                new List<IngredientElement>
                {
                    new("Rock", 1, true),
                },
                new List<CraftingElement>
                {
                    new CraftingElement<StoneWhetstoneItem>()
                });
            Recipes = new List<Recipe> {recipe};
            ExperienceOnCraft = 5;
            LaborInCalories = CreateLaborInCaloriesValue(100, typeof(BasicEngineeringSkill));
            CraftMinutes = CreateCraftTimeValue(typeof(StoneWhetstoneRecipe), 1, typeof(BasicEngineeringSkill));
            Initialize(Localizer.DoStr("Stone Whetstone"), typeof(StoneWhetstoneRecipe));
            CraftingComponent.AddRecipe(typeof(WorkbenchObject), this);
        }
    }
}