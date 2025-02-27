﻿#pragma warning disable CS1998 // async without await
#pragma warning disable BL0005

using Bunit;
using MudExtensions.UnitTests.TestComponents;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Extensions;

namespace MudExtensions.UnitTests.Components
{
    [TestFixture]
    public class ListExtendedTests : BunitTest
    {
        [Test]
        public async Task List_EventCountTest()
        {
            var comp = Context.RenderComponent<ListExperimentalCountTest>();
            var list = comp.FindComponent<MudListExtended<int?>>().Instance;

            comp.Instance.ValueChangeCount.Should().Be(0);
            comp.Instance.ValuesChangeCount.Should().Be(0);
            comp.Instance.ItemChangeCount.Should().Be(0);
            comp.Instance.ItemsChangeCount.Should().Be(0);

            await comp.InvokeAsync(() => list.SelectedValue = 1);
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(1));
            comp.Instance.ValuesChangeCount.Should().Be(1);
            comp.Instance.ItemChangeCount.Should().Be(1);
            comp.Instance.ItemsChangeCount.Should().Be(1);
            // Clicking the current item should not fire events
            await comp.InvokeAsync(() => comp.FindAll("div.mud-list-item-extended")[0].Click());
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(1));
            comp.Instance.ValuesChangeCount.Should().Be(1);
            comp.Instance.ItemChangeCount.Should().Be(1);
            comp.Instance.ItemsChangeCount.Should().Be(1);

            await comp.InvokeAsync(() => comp.FindAll("div.mud-list-item-extended")[1].Click());
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(2));
            comp.Instance.ValuesChangeCount.Should().Be(2);
            comp.Instance.ItemChangeCount.Should().Be(2);
            comp.Instance.ItemsChangeCount.Should().Be(2);

            await comp.InvokeAsync(() => list.SelectedItem = list.GetItems()[3]);
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(3));
            comp.Instance.ValuesChangeCount.Should().Be(3);
            comp.Instance.ItemChangeCount.Should().Be(3);
            comp.Instance.ItemsChangeCount.Should().Be(3);
            // Setting same item shold not fire events
            await comp.InvokeAsync(() => list.SelectedItem = list.GetItems()[3]);
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(3));
            comp.Instance.ValuesChangeCount.Should().Be(3);
            comp.Instance.ItemChangeCount.Should().Be(3);
            comp.Instance.ItemsChangeCount.Should().Be(3);

            await comp.InvokeAsync(() => list.MultiSelection = true);

            await comp.InvokeAsync(() => list.SelectedValues = new HashSet<int?>() { 1, 6 });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(4));
            comp.Instance.ValuesChangeCount.Should().Be(4);
            comp.Instance.ItemChangeCount.Should().Be(4);
            comp.Instance.ItemsChangeCount.Should().Be(4);
            // SelectedValue takes last value on SelectedValues, so if changed values have same last item, value and item should not change
            await comp.InvokeAsync(() => list.SelectedValues = new HashSet<int?>() { 2, 6 });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(4));
            comp.Instance.ValuesChangeCount.Should().Be(5);
            comp.Instance.ItemChangeCount.Should().Be(4);
            comp.Instance.ItemsChangeCount.Should().Be(5);
            // Last value changed, so expected to all events fire
            await comp.InvokeAsync(() => list.SelectedValues = new HashSet<int?>() { 2, 5 });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(5));
            comp.Instance.ValuesChangeCount.Should().Be(6);
            comp.Instance.ItemChangeCount.Should().Be(5);
            comp.Instance.ItemsChangeCount.Should().Be(6);
            // Changing to same value should not fire any events
            await comp.InvokeAsync(() => list.SelectedValues = new HashSet<int?>() { 2, 5 });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(5));
            comp.Instance.ValuesChangeCount.Should().Be(6);
            comp.Instance.ItemChangeCount.Should().Be(5);
            comp.Instance.ItemsChangeCount.Should().Be(6);

            await comp.InvokeAsync(() => list.SelectedItems = new HashSet<MudListItemExtended<int?>>() { list.GetItems()[0], list.GetItems()[3] });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(6));
            comp.Instance.ValuesChangeCount.Should().Be(7);
            comp.Instance.ItemChangeCount.Should().Be(6);
            comp.Instance.ItemsChangeCount.Should().Be(7);
            // Same for selected values, if last value doesn't change, value event should not fire
            await comp.InvokeAsync(() => list.SelectedItems = new HashSet<MudListItemExtended<int?>>() { list.GetItems()[1], list.GetItems()[3] });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(6));
            comp.Instance.ValuesChangeCount.Should().Be(8);
            comp.Instance.ItemChangeCount.Should().Be(6);
            comp.Instance.ItemsChangeCount.Should().Be(8);
            // Changing to the same should not fire any events
            await comp.InvokeAsync(() => list.SelectedItems = new HashSet<MudListItemExtended<int?>>() { list.GetItems()[1], list.GetItems()[3] });
            comp.WaitForAssertion(() => comp.Instance.ValueChangeCount.Should().Be(6));
            comp.Instance.ValuesChangeCount.Should().Be(8);
            comp.Instance.ItemChangeCount.Should().Be(6);
            comp.Instance.ItemsChangeCount.Should().Be(8);
        }

        /// <summary>
        /// Clicking the drinks selects them. The child lists are updated accordingly, meaning, only ever 1 list item can have the active class.
        /// 
        /// In this test no item is selected to begin with
        /// </summary>
        [Test]
        public async Task ListSelectionTest()
        {
            var comp = Context.RenderComponent<ListExperimentalSelectionTest>();
            //Console.WriteLine(comp.Markup);
            var list = comp.FindComponent<MudListExtended<int>>().Instance;
            list.SelectedItem.Should().Be(null);
            // we have seven choices, none is active
            comp.FindAll("div.mud-list-item-extended").Count.Should().Be(9); // 7 choices, 2 groups 
            comp.FindAll("div.mud-selected-item").Count.Should().Be(0); //nested lists generate 1 selected item tag
            // click water
            comp.FindAll("div.mud-list-item-extended")[0].Click();
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Sparkling Water"));
            comp.FindAll("div.mud-selected-item").Count.Should().Be(1);
            comp.FindComponents<MudListItemExtended<int>>()[0].Markup.Should().Contain("mud-selected-item");
            // click Pu'er, a heavily fermented Chinese tea that tastes like an old leather glove
            comp.FindAll("div.mud-list-item-extended")[4].Click();
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Pu'er"));
            comp.FindAll("div.mud-selected-item").Count.Should().Be(1);
            comp.FindComponents<MudListItemExtended<int>>()[4].Markup.Should().Contain("mud-selected-item");
            // click Cafe Latte
            comp.FindAll("div.mud-list-item-extended")[8].Click();
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Cafe Latte"));
            comp.FindAll("div.mud-selected-item").Count.Should().Be(1);
            comp.FindComponents<MudListItemExtended<int>>()[8].Markup.Should().Contain("mud-selected-item");
        }

        [Test]
        public async Task ListMultiSelectionTest()
        {
            var comp = Context.RenderComponent<ListExperimentalSelectionTest>();
            //Console.WriteLine(comp.Markup);
            var list = comp.FindComponent<MudListExtended<int>>().Instance;
            list.SelectedItem.Should().Be(null);
            // we have seven choices, none is active
            comp.FindAll("div.mud-list-item-extended").Count.Should().Be(9); // 7 choices, 2 groups 
            comp.FindAll("div.mud-selected-item").Count.Should().Be(0); //nested lists generate 1 selected item tag
            // click water
            comp.FindAll("div.mud-list-item-extended")[0].Click();
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Sparkling Water"));
            comp.FindAll("div.mud-selected-item").Count.Should().Be(1);
            comp.FindComponents<MudListItemExtended<int>>()[0].Markup.Should().Contain("mud-selected-item");
            list.MultiSelection = true;
            // click Pu'er, a heavily fermented Chinese tea that tastes like an old leather glove
            comp.FindAll("div.mud-list-item-extended")[4].Click();
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Pu'er"));
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(2));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Markup.Should().Contain("mud-selected-item"));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[4].Markup.Should().Contain("mud-selected-item"));
            // click Cafe Latte
            comp.FindAll("div.mud-list-item-extended")[8].Click();
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Cafe Latte"));
            comp.FindAll("div.mud-selected-item").Count.Should().Be(3);
            comp.FindComponents<MudListItemExtended<int>>()[0].Markup.Should().Contain("mud-selected-item");
            comp.FindComponents<MudListItemExtended<int>>()[4].Markup.Should().Contain("mud-selected-item");
            comp.FindComponents<MudListItemExtended<int>>()[8].Markup.Should().Contain("mud-selected-item");

            comp.FindAll("div.mud-list-item-extended")[4].Click();
            comp.FindAll("div.mud-selected-item").Count.Should().Be(2);
            comp.FindComponents<MudListItemExtended<int>>()[0].Markup.Should().Contain("mud-selected-item");
            comp.FindComponents<MudListItemExtended<int>>()[8].Markup.Should().Contain("mud-selected-item");

            await comp.InvokeAsync(() => list.Clear());
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(0));
        }

        /// <summary>
        /// Clicking the drinks selects them. The child lists are updated accordingly, meaning, only ever 1 list item can have the active class.
        /// 
        /// This test starts with a pre-selected item (by value)
        /// </summary>
        [Test]
        public async Task ListWithPreSelectedValueTest()
        {
            var comp = Context.RenderComponent<ListExperimentalSelectionInitialValueTest>();
            //Console.WriteLine(comp.Markup);
            var list = comp.FindComponent<MudListExtended<int?>>().Instance;
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int?>>()[0].Markup.Should().Contain("mud-selected-item"));
            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(1));
            comp.WaitForAssertion(() => list.SelectedValues.Should().Contain(1));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Sparkling Water"));
            comp.WaitForAssertion(() => list.SelectedItems?.First().Text.Should().Be("Sparkling Water"));
            // we have seven choices, 1 is active because of the initial value of SelectedValue
            comp.WaitForAssertion(() => comp.FindAll("div.mud-list-item-extended").Count.Should().Be(9)); // 7 choices, 2 groups
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(1));
            // set Pu'er, a heavily fermented Chinese tea that tastes like an old leather glove
            await comp.InvokeAsync(() => comp.Instance.SetSelectedValue(4));
            //await comp.InvokeAsync(() => list.HandleCentralValueCommander("SelectedValue"));
            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(4));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Pu'er"));
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(1));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int?>>()[4].Markup.Should().Contain("mud-selected-item"));
            // set Cafe Latte via changing SelectedValue
            await comp.InvokeAsync(() => comp.Instance.SetSelectedValue(7));
            //await comp.InvokeAsync(() => list.HandleCentralValueCommander("SelectedValue"));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Cafe Latte"));
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(1));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int?>>()[8].Markup.Should().Contain("mud-selected-item"));
            // set water
            await comp.InvokeAsync(() => comp.Instance.SetSelectedValue(1));
            //await comp.InvokeAsync(() => list.HandleCentralValueCommander("SelectedValue"));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Sparkling Water"));
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(1));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int?>>()[0].Markup.Should().Contain("mud-selected-item"));
            // set nothing
            await comp.InvokeAsync(() => comp.Instance.SetSelectedValue(null));
            //await comp.InvokeAsync(() => list.HandleCentralValueCommander("SelectedValue"));
            comp.WaitForAssertion(() => list.SelectedItem?.Should().Be(null));
            comp.WaitForAssertion(() => comp.FindAll("div.mud-selected-item").Count.Should().Be(0));
        }

        [Test]
        public async Task List_ProgrammaticallyChangeValueAndItemTest()
        {
            var comp = Context.RenderComponent<ListExperimentalVariantTest>();
            var list = comp.FindComponent<MudListExtended<int?>>().Instance;

            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(1));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Sparkling Water (1)"));

            comp.FindAll("button.mud-button-root")[1].Click();
            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(2));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Still Water (2)"));

            comp.FindAll("button.mud-button-root")[3].Click();
            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(3));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Earl Grey (3)"));

            // Changing multiselection should not affect value or item
            await comp.InvokeAsync(() => list.MultiSelection = true);
            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(3));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Earl Grey (3)"));

            comp.FindAll("button.mud-button-root")[2].Click();
            comp.WaitForAssertion(() => list.SelectedValues.Should().ContainInOrder(new int?[] { 2, 4 }));
            comp.WaitForAssertion(() => string.Join(", ", list.SelectedItems?.Select(x => x.Text) ?? new List<string>()).Should().Be("Still Water (2), Matcha (4)"));

            comp.FindAll("button.mud-button-root")[4].Click();
            comp.WaitForAssertion(() => list.SelectedValues.Should().ContainInOrder(new int?[] { 3, 5 }));
            comp.WaitForAssertion(() => string.Join(", ", list.SelectedItems?.Select(x => x.Text) ?? new List<string>()).Should().Be("Earl Grey (3), Pu'er (5)"));

            // Changing multiselection now should select only one value
            await comp.InvokeAsync(() => list.MultiSelection = false);
            comp.WaitForAssertion(() => list.SelectedValue.Should().Be(3));
            comp.WaitForAssertion(() => list.SelectedItem?.Text.Should().Be("Earl Grey (3)"));
            comp.WaitForAssertion(() => list.SelectedValues.Should().ContainSingle());
            comp.WaitForAssertion(() => string.Join(", ", list.SelectedItems?.Select(x => x.Text) ?? new List<string>()).Should().Be("Earl Grey (3)"));
        }

        [Test]
        public async Task List_KeyboardNavigationTest()
        {
            var comp = Context.RenderComponent<ListExperimentalEnhancedTest>();
            //Console.WriteLine(comp.Markup);
            var list = comp.FindComponent<MudListExtended<int>>().Instance;

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "ArrowDown" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().BeNullOrEmpty());

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "Enter" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().ContainSingle());
            //Second item is functional, should skip.
            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "ArrowDown" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Instance.IsActive.Should().BeFalse());
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[2].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().ContainSingle());

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "NumpadEnter" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[2].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().ContainInOrder(new int[] { 1, 3 }));

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "ArrowDown" }));
            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "ArrowDown" }));
            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "Enter" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[2].Instance.IsActive.Should().BeFalse());
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[5].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(3));
            comp.WaitForAssertion(() => list.SelectedValues.Should().Contain(5));

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "Enter" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[5].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(2));
            comp.WaitForAssertion(() => list.SelectedValues.Should().Contain(3));
            //Last disabled item should not be active
            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "End" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[9].Instance.IsActive.Should().BeTrue());

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "Home" }));
            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "Enter" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(1));
            comp.WaitForAssertion(() => list.SelectedValues.Should().NotContain(1));

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "a", CtrlKey = true }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(7));
            comp.WaitForAssertion(() => list.SelectedValues.Should().NotContain(2));

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "A", CtrlKey = true }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[0].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(0));

            await comp.InvokeAsync(() => comp.FindAll("div.mud-list-item-extended")[5].Click());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(1));
            comp.WaitForAssertion(() => list.SelectedValues.Should().Contain(5));

            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "ArrowDown" }));
            await comp.InvokeAsync(() => list.HandleKeyDownAsync(new KeyboardEventArgs() { Key = "Enter" }));
            comp.WaitForAssertion(() => comp.FindComponents<MudListItemExtended<int>>()[6].Instance.IsActive.Should().BeTrue());
            comp.WaitForAssertion(() => list.SelectedValues.Should().HaveCount(2));
            comp.WaitForAssertion(() => list.SelectedValues.Should().Contain(6));
        }

        [Test]
        [TestCase(Color.Default)]
        [TestCase(Color.Primary)]
        [TestCase(Color.Secondary)]
        [TestCase(Color.Tertiary)]
        [TestCase(Color.Info)]
        [TestCase(Color.Success)]
        [TestCase(Color.Warning)]
        [TestCase(Color.Error)]
        [TestCase(Color.Dark)]
        public void ListColorTest(Color color)
        {
            var comp = Context.RenderComponent<ListExperimentalSelectionInitialValueTest>(x => x.Add(c => c.Color, color));
            var list = comp.FindComponent<MudListExtended<int?>>().Instance;
            list.SelectedItem?.Text.Should().Be("Sparkling Water");

            var listItemClasses = comp.Find(".mud-selected-item");
            listItemClasses.ClassList.Should().ContainInOrder(new[] { $"mud-{color.ToDescriptionString()}-text", $"mud-{color.ToDescriptionString()}-hover" });
        }
    }
}
