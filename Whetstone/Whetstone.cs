#pragma warning disable CA1416

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public override string OnUsed(Player player, ItemStack itemStack)
        {
            var selectedItem = player.User.Inventory.Toolbar.SelectedItem;
            if (selectedItem is not ToolItem tool) return string.Empty;
            tool.Durability = 0;
            return base.OnUsed(player, itemStack);
        }
    }

    public abstract class Whetstone : Item
    {
        public const float CraftTime = 0.5f;
        protected virtual Tag RepairTag => null;
        protected virtual Type RepairItem => null;

        public override string OnUsed(Player player, ItemStack itemStack)
        {
            var selectedItem = player.User.Inventory.Toolbar.SelectedItem;
            if (selectedItem == null)
            {
                player.Error(Localizer.DoStr($"Missing repair target. Please select a tool on the toolbar"));
                return string.Empty;
            }
            if (selectedItem is not RepairableItem repairable || !CanRepair(repairable))
            {
                player.Error(Localizer.DoStr($"{DisplayName} cannot fix {selectedItem.DisplayName}!"));
                return string.Empty;
            }

            if (!NeedRepair(repairable))
            {
                player.Error(Localizer.DoStr($"{selectedItem.DisplayName} is all good"));
                return string.Empty;
            }

            itemStack.TryModifyStack(player.User, -1, null, () =>
            {
                repairable.Durability = 100;
                player.InfoBox(Localizer.DoStr($"{selectedItem.DisplayName} repaired to full durability!"));
            });
            return base.OnUsed(player, itemStack);
        }

        private bool CanRepair(RepairableItem tool)
        {
            return CanRepairByItem(tool) || CanRepairByTag(tool);
        }

        private bool CanRepairByItem(RepairableItem tool)
        {
            return tool.RepairItem != null && tool.RepairItem.GetType() == RepairItem;
        }

        private bool CanRepairByTag(RepairableItem tool)
        {
            return tool.RepairTag != null && tool.RepairTag == RepairTag;
        }

        private static bool NeedRepair(DurabilityItem item)
        {
            return item.GetDurability() < DurabilityItem.DurabilityMax;
        }
    }

    [Serialized]
    [LocDisplayName("Wood Whetstone")]
    [Weight(500)]
    [MaxStackSize(50)]
    [Currency]
    public class WoodWhetstoneItem : Whetstone
    {
        protected override Tag RepairTag => Tag.Wood;
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair wooden tools");
    }

    [Serialized]
    [LocDisplayName("Stone Whetstone")]
    [Weight(500)]
    [MaxStackSize(50)]
    [Currency]
    public class StoneWhetstoneItem : Whetstone
    {
        protected override Tag RepairTag => TagManager.Tag("Rock");
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair stone tools");
    }

    [Serialized]
    [LocDisplayName("Iron Whetstone")]
    [Weight(500)]
    [MaxStackSize(50)]
    [Currency]
    public class IronWhetstoneItem : Whetstone
    {
        protected override Type RepairItem => typeof(IronBarItem);

        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair iron tools");
    }

    [Serialized]
    [LocDisplayName("Steel Whetstone")]
    [Weight(500)]
    [MaxStackSize(50)]
    [Currency]
    public class SteelWhetstoneItem : Whetstone
    {
        protected override Type RepairItem => typeof(SteelBarItem);
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair modern/steel tools");
    }

    [Serialized]
    [LocDisplayName("Tractor Module Repair Kit")]
    [Weight(500)]
    [MaxStackSize(50)]
    [Currency]
    public class TractorModuleRepairKitItem : Whetstone
    {
        protected override Type RepairItem => typeof(IronPlateItem);
        public override LocString DisplayDescription => Localizer.DoStr("Consume to repair tractor modules");
    }

    [RequiresSkill(typeof(BasicEngineeringSkill), 0)]
    public class WoodWhetstoneRecipe : RecipeFamily
    {
        public static int InferRepairCostBase(RepairableItem item)
        {
            return (int)Math.Ceiling(item.FullRepairAmount * 0.2);
        }
        public WoodWhetstoneRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                "Wood Whetstone",
                Localizer.DoStr("Wood Whetstone"),
                new List<IngredientElement>
                {
                    new("Wood",
                        InferRepairCostBase(new WoodenShovelItem()),
                        typeof(BasicEngineeringSkill),
                        typeof(BasicEngineeringLavishResourcesTalent)),
                },
                new List<CraftingElement>
                {
                    new CraftingElement<WoodWhetstoneItem>(),
                });
            Recipes = new List<Recipe> { recipe };
            ExperienceOnCraft = 1;
            LaborInCalories = CreateLaborInCaloriesValue(100);
            CraftMinutes = CreateCraftTimeValue(
            typeof(WoodWhetstoneItem),
            Whetstone.CraftTime,
            typeof(BasicEngineeringSkill),
            typeof(BasicEngineeringFocusedSpeedTalent),
            typeof(BasicEngineeringParallelSpeedTalent));
            Initialize(Localizer.DoStr("Wood Whetstone"), typeof(WoodWhetstoneRecipe));
            CraftingComponent.AddRecipe(typeof(ToolBenchObject), this);
        }
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
                    new("Rock",
                        WoodWhetstoneRecipe.InferRepairCostBase(new StonePickaxeItem()),
                        typeof(BasicEngineeringSkill),
                        typeof(BasicEngineeringLavishResourcesTalent)),
                },
                new List<CraftingElement>
                {
                    new CraftingElement<StoneWhetstoneItem>()
                });
            Recipes = new List<Recipe> { recipe };
            ExperienceOnCraft = 1;
            LaborInCalories = CreateLaborInCaloriesValue(100);
            CraftMinutes = CreateCraftTimeValue(
                typeof(StoneWhetstoneRecipe),
                Whetstone.CraftTime,
                typeof(BasicEngineeringSkill),
                typeof(BasicEngineeringFocusedSpeedTalent),
                typeof(BasicEngineeringParallelSpeedTalent));
            Initialize(Localizer.DoStr("Stone Whetstone"), typeof(StoneWhetstoneRecipe));
            CraftingComponent.AddRecipe(typeof(ToolBenchObject), this);
        }
    }

    [RequiresSkill(typeof(SmeltingSkill), 1)]
    public class IronWhetstoneRecipe : RecipeFamily
    {
        public IronWhetstoneRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                "Iron Whetstone",
                Localizer.DoStr("Iron Whetstone"),
                new List<IngredientElement>
                {
                    new(typeof(IronBarItem),
                        WoodWhetstoneRecipe.InferRepairCostBase(new IronPickaxeItem()),
                        typeof(SmeltingSkill),
                        typeof(SmeltingLavishResourcesTalent)),
                },
                new List<CraftingElement>
                {
                    new CraftingElement<IronWhetstoneItem>()
                });
            Recipes = new List<Recipe> { recipe };
            ExperienceOnCraft = 1;
            LaborInCalories = CreateLaborInCaloriesValue(100, typeof(SmeltingSkill));
            CraftMinutes = CreateCraftTimeValue(
                typeof(IronWhetstoneItem),
                Whetstone.CraftTime,
                typeof(SmeltingSkill),
                typeof(SmeltingFocusedSpeedTalent),
                typeof(SmeltingParallelSpeedTalent));
            Initialize(Localizer.DoStr("Iron Whetstone"), typeof(IronWhetstoneRecipe));
            CraftingComponent.AddRecipe(typeof(AnvilObject), this);
        }
    }

    [RequiresSkill(typeof(AdvancedSmeltingSkill), 1)]
    public class SteelWhetstoneRecipe : RecipeFamily
    {
        public SteelWhetstoneRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                "Steel Whetstone",
                Localizer.DoStr("Steel Whetstone"),
                new List<IngredientElement>
                {
                    new(typeof(SteelBarItem),
                        WoodWhetstoneRecipe.InferRepairCostBase(new ModernPickaxeItem()),
                        typeof(AdvancedSmeltingSkill),
                        typeof(AdvancedSmeltingLavishResourcesTalent)),
                },
                new List<CraftingElement>
                {
                    new CraftingElement<SteelWhetstoneItem>()
                });
            Recipes = new List<Recipe> { recipe };
            ExperienceOnCraft = 1;
            LaborInCalories = CreateLaborInCaloriesValue(200, typeof(AdvancedSmeltingSkill));
            CraftMinutes = CreateCraftTimeValue(
                typeof(SteelWhetstoneItem),
                Whetstone.CraftTime,
                typeof(AdvancedSmeltingSkill),
                typeof(AdvancedSmeltingFocusedSpeedTalent),
                typeof(AdvancedSmeltingParallelSpeedTalent));
            Initialize(Localizer.DoStr("Steel Whetstone"), typeof(SteelWhetstoneRecipe));
            CraftingComponent.AddRecipe(typeof(AnvilObject), this);
        }
    }

    [RequiresSkill(typeof(MechanicsSkill), 1)]
    public class TractorModuleRepairKitRecipe : RecipeFamily
    {
        public TractorModuleRepairKitRecipe()
        {
            var recipe = new Recipe();
            recipe.Init(
                "Tractor Module Repair Kit",
                Localizer.DoStr("Tractor Module Repair Kit"),
                new List<IngredientElement>
                {
                    new(typeof(IronPlateItem),
                        WoodWhetstoneRecipe.InferRepairCostBase(new SteamTractorHarvesterItem()),
                        typeof(MechanicsSkill),
                        typeof(MechanicsLavishResourcesTalent)),
                },
                new List<CraftingElement>
                {
                    new CraftingElement<TractorModuleRepairKitItem>()
                });
            Recipes = new List<Recipe> { recipe };
            ExperienceOnCraft = 1;
            LaborInCalories = CreateLaborInCaloriesValue(200, typeof(MechanicsSkill));
            CraftMinutes = CreateCraftTimeValue(
                typeof(TractorModuleRepairKitItem),
                Whetstone.CraftTime,
                typeof(MechanicsSkill),
                typeof(MechanicsFocusedSpeedTalent),
                typeof(MechanicsParallelSpeedTalent));
            Initialize(Localizer.DoStr("Tractor Module Repair Kit"), typeof(TractorModuleRepairKitRecipe));
            CraftingComponent.AddRecipe(typeof(MachinistTableObject), this);
        }
    }
}
