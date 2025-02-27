﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Services;
using MudBlazor.Utilities;
using System.Runtime.InteropServices;

namespace MudExtensions
{
    /// <summary>
    /// Component that has select and autocomplete features.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class MudComboBox<T> : MudBaseInputExtended<T>
    {
        #region Constructor, Injected Services, Parameters, Fields

        /// <summary>
        /// Constructor for ComboBox
        /// </summary>
        public MudComboBox()
        {
            Adornment = Adornment.End;
            IconSize = Size.Medium;
            //base.SkipUpdateProcessOnSetParameters = true;
        }

        /// <summary>
        /// Protected search string converter.
        /// </summary>
        /// <param name="value"></param>
        protected internal void SetSearchString(T value)
        {
            _searchString = Converter.Set(value);
        }

        internal string? _searchString { get; set; }

        /// <summary>
        /// Item list of ComboBox.
        /// </summary>
        public List<MudComboBoxItem<T>> Items { get; set; } = new();
        internal MudComboBoxItem<T>? _lastActivatedItem;

        /// <summary>
        /// Eligible items of ComboBox.
        /// </summary>
        protected internal List<MudComboBoxItem<T>> EligibleItems { get; set; } = new();
        private MudInputExtended<string?> _inputReference = new();
        private MudTextFieldExtended<string?> _searchbox = new();
        internal bool _isOpen;

        /// <summary>
        /// Icon string.
        /// </summary>
        protected internal string? _currentIcon { get; set; }

        /// <summary>
        /// Main CSS classes.
        /// </summary>
        protected string? Classname =>
            new CssBuilder("mud-select-extended")
            .AddClass(Class)
            .Build();

        /// <summary>
        /// Input CSS classes.
        /// </summary>
        protected string InputClassname =>
            new CssBuilder("mud-select-input-extended")
            .AddClass(InputClass)
            .Build();

        /// <summary>
        /// CSS classes for popover content.
        /// </summary>
        protected string PopoverClassname =>
            new CssBuilder()
            .AddClass("d-none", _isOpen == false)
            .AddClass(PopoverClass)
            .Build();

        /// <summary>
        /// CSS styles for mock input.
        /// </summary>
        protected string MockInputStylename =>
            new StyleBuilder()
            .AddStyle("height: auto")
            .AddStyle("min-height: 1.1876em")
            .AddStyle("display", "inline", Value != null || SelectedValues?.Any() == true)
            .Build();

        private readonly string _elementId = string.Concat("combobox_", Guid.NewGuid().ToString().AsSpan(0, 8));
        private readonly string _popoverId = string.Concat("comboboxpopover_", Guid.NewGuid().ToString().AsSpan(0, 8));

        /// <summary>
        /// If true, combobox goes to autocomplete mode.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public bool Editable { get; set; }

        /// <summary>
        /// If false, all items are eligible regarding what user search in textfield.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public bool EnableFilter { get; set; } = true;

        /// <summary>
        /// If true, searched text has highlight.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public bool Highlight { get; set; }

        /// <summary>
        /// Overrides the highlight class.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public string? HighlightClass { get; set; }

        /// <summary>
        /// If true, selected or active items in popover has border.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public bool Bordered { get; set; }

        /// <summary>
        /// User class names for the input, separated by space.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public string? InputClass { get; set; }

        /// <summary>
        /// User style names for the input, separated by space.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Appearance)]
        [Parameter] public string? InputStyle { get; set; }

        /// <summary>
        /// Fired when dropdown opens.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Behavior)]
        [Parameter] public EventCallback OnOpen { get; set; }

        /// <summary>
        /// Fired when dropdown closes.
        /// </summary>
        [Category(CategoryTypes.FormComponent.Behavior)]
        [Parameter] public EventCallback OnClose { get; set; }

        /// <summary>
        /// Add the MudComboBoxItems here.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Content that placed on top in popover.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public RenderFragment? PopoverStartContent { get; set; }

        /// <summary>
        /// Content that placed on bottom in popover.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public RenderFragment? PopoverEndContent { get; set; }

        /// <summary>
        /// Content that shown when no items found.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public RenderFragment? NoItemsContent { get; set; }

        /// <summary>
        /// Optional presentation template for items
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public RenderFragment<MudComboBoxItem<T>>? ItemTemplate { get; set; }

        /// <summary>
        /// Optional presentation template for selected items
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public RenderFragment<MudComboBoxItem<T>>? ItemSelectedTemplate { get; set; }

        /// <summary>
        /// Optional presentation template for disabled items
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public RenderFragment<MudComboBoxItem<T>>? ItemDisabledTemplate { get; set; }

        /// <summary>
        /// Function to be invoked when checking whether an item should be disabled or not. Works both with renderfragment and ItemCollection approach.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public Func<T?, bool>? ItemDisabledFunc { get; set; }

        /// <summary>
        /// Classname for item template or chips.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public string? TemplateClass { get; set; }

        /// <summary>
        /// If true the active (hilighted) item select on tab key. Designed for only single selection. Default is false.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Selecting)]
        public bool SelectValueOnTab { get; set; }

        /// <summary>
        /// User class names for the popover, separated by space
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public string? PopoverClass { get; set; }

        /// <summary>
        /// The text that shown on empty searchbox.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public string? SearchBoxPlaceholder { get; set; }

        /// <summary>
        /// If true, compact vertical padding will be applied to all Select items.
        /// </summary>
        /// <remarks>The default is <see cref="Dense.Standard"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public Dense Dense { get; set; } = Dense.Standard;

        /// <summary>
        /// The Open Select Icon
        /// </summary>
        /// <remarks>The default is <see cref="Icons.Material.Filled.ArrowDropDown"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string? OpenIcon { get; set; } = Icons.Material.Filled.ArrowDropDown;

        /// <summary>
        /// The Close Select Icon
        /// </summary>
        /// <remarks>The default is <see cref="Icons.Material.Filled.ArrowDropUp"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string? CloseIcon { get; set; } = Icons.Material.Filled.ArrowDropUp;

        /// <summary>
        /// Dropdown color of select. Supports theme colors.
        /// </summary>
        /// <remarks>The default is <see cref="Color.Primary"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public Color Color { get; set; } = Color.Primary;

        /// <summary>
        /// The color of the text. It supports the theme colors.
        /// </summary>
        /// <remarks>The default is <see cref="Color.Default"/></remarks>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public Color TextColor { get; set; } = Color.Default;

        /// <summary>
        /// The color of the checked checkbox. It supports the theme colors.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public Color? CheckBoxCheckedColor { get; set; } = null;

        /// <summary>
        /// The color of the unchecked checkbox. It supports the theme colors.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Radio.Appearance)]
        public Color? CheckBoxUnCheckedColor { get; set; } = null;

        /// <summary>
        /// The size of the checkbox.
        /// </summary>
        /// <remarks>The default is <see cref="Size.Medium"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public Size CheckBoxSize { get; set; } = Size.Medium;

        /// <summary>
        /// The input's visual.
        /// </summary>
        /// <remarks>The default is <see cref="ValuePresenter.Text"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public ValuePresenter InputPresenter { get; set; } = ValuePresenter.Text;

        /// <summary>
        /// The items' visual in popover.
        /// </summary>
        /// <remarks>The default is <see cref="ValuePresenter.Text"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public ValuePresenter ItemPresenter { get; set; } = ValuePresenter.Text;

        /// <summary>
        /// If true, shows checkbox when multiselection is true.
        /// </summary>
        /// <remarks>The default is <c>true</c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public bool ShowCheckbox { get; set; } = true;

        /// <summary>
        /// If set to true and the MultiSelection option is set to true, a "select all" checkbox is added at the top of the list of items.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public bool SelectAll { get; set; }

        /// <summary>
        /// Sets position of the Select All checkbox
        /// </summary>
        /// <remarks>The default is <see cref="SelectAllPosition.BeforeSearchBox"/></remarks>
        [Parameter]
        [Category(CategoryTypes.List.Appearance)]
        public SelectAllPosition SelectAllPosition { get; set; } = SelectAllPosition.BeforeSearchBox;

        /// <summary>
        /// Define the text of the Select All option.
        /// </summary>
        /// <remarks>The default is <c>Select All</c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public string SelectAllText { get; set; } = "Select All";

        /// <summary>
        /// Function to define a customized multiselection text.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public Func<List<T?>, string>? MultiSelectionTextFunc { get; set; }

        /// <summary>
        /// Custom search func for searchbox. If doesn't set, it search with "Contains" logic by default. Passed value and searchString values.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public Func<T?, string?, string?, bool>? SearchFunc { get; set; }

        //[Parameter]
        //[Category(CategoryTypes.FormComponent.Behavior)]
        //public ICollection<T> ItemCollection { get; set; } = null;

        //[Parameter]
        //[Category(CategoryTypes.List.Behavior)]
        //public bool Virtualize { get; set; }

        /// <summary>
        /// If true, chips has close button and remove from SelectedValues when pressed the close button.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public bool ChipCloseable { get; set; }

        /// <summary>
        /// CSS classes for chips.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public string? ChipClass { get; set; }

        /// <remarks>The default is <see cref="Variant.Filled"/></remarks>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public Variant ChipVariant { get; set; } = Variant.Filled;

        /// <remarks>The default is <see cref="Size.Small"/></remarks>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public Size ChipSize { get; set; } = Size.Small;

        /// <summary>
        /// Parameter to define the delimited string separator.
        /// </summary>
        /// <remarks>The default is <c>, </c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public string? Delimiter { get; set; } = ", ";

        /// <summary>
        /// If true popover width will be the same as the combobox component.
        /// </summary>
        /// <remarks>The default is <c>true</c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public DropdownWidth RelativeWidth { get; set; } = DropdownWidth.Relative;

        /// <summary>
        /// Sets the maxheight of the popover.
        /// </summary>
        /// <remarks>The default is <c>300</c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public int MaxHeight { get; set; } = 300;

        /// <summary>
        /// Set the anchor origin point to determine where the popover will open from.
        /// </summary>
        /// <remarks>The default is <see cref="Origin.BottomCenter"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public Origin AnchorOrigin { get; set; } = Origin.BottomCenter;

        /// <summary>
        /// Sets the transform origin point for the popover.
        /// </summary>
        /// <remarks>The default is <see cref="Origin.TopCenter"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public Origin TransformOrigin { get; set; } = Origin.TopCenter;

        /// <summary>
        /// If false, combobox allows value from out of bound.
        /// </summary>
        /// <remarks>The default is <c>true</c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Strict { get; set; } = true;

        /// <summary>
        /// Show clear button.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Clearable { get; set; }

        /// <summary>
        /// If <c>true</c> will open the menu when the clear button is clicked.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public bool OpenMenuAfterClear { get; set; }

        /// <summary>
        /// If true, shows a searchbox for filtering items. Only works with ItemCollection approach.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public bool SearchBox { get; set; }

        /// <summary>
        /// If true, the search-box will be focused when the dropdown is opened.
        /// </summary>
        /// <remarks>The default is <c>true</c></remarks>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public bool SearchBoxAutoFocus { get; set; } = true;

        /// <summary>
        /// If true, the search-box has a clear icon.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public bool SearchBoxClearable { get; set; }

        /// <summary>
        /// If true, prevent scrolling while dropdown is open.
        /// </summary>
        /// <remarks>The default is <c>true</c></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public bool LockScroll { get; set; } = true;

        /// <summary>
        /// Button click event for clear button. Called after text and value has been cleared.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClearButtonClick { get; set; }

        /// <summary>
        /// Custom indeterminate icon.
        /// </summary>
        /// <remarks>The default is <see cref="Icons.Material.Filled.IndeterminateCheckBox"/></remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListAppearance)]
        public string IndeterminateIcon { get; set; } = Icons.Material.Filled.IndeterminateCheckBox;

        /// <summary>
        /// If true, multiple values can be selected via checkboxes which are automatically shown in the dropdown
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public bool MultiSelection { get; set; }

        /// <summary>
        /// If true, clicking on selected item can unselect the item.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public bool ToggleSelection { get; set; }

        /// <summary>
        /// Protected selectedvalues sync.
        /// </summary>
        /// <param name="singleToMultiselection"></param>
        /// <returns></returns>
        protected async Task SyncMultiselectionValues(bool singleToMultiselection)
        {
            if (singleToMultiselection == true)
            {
                if (Value == null)
                {
                    SelectedValues = new HashSet<T?>();
                }
                else
                {
                    if (Value is string && string.IsNullOrWhiteSpace(Converter.Set(Value)))
                    {
                        SelectedValues = new HashSet<T?>();
                    }
                    else
                    {
                        SelectedValues = new HashSet<T?>() { Value };
                    }

                }
                await SelectedValuesChanged.InvokeAsync(_selectedValues);
            }
            else
            {
                await SetValueAsync(SelectedValues.LastOrDefault(), false);
                _searchString = Converter.Set(SelectedValues.LastOrDefault());
            }
        }

        private IEqualityComparer<T?>? _comparer;
        /// <summary>
        /// The Comparer to use for comparing selected values internally.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public IEqualityComparer<T?>? Comparer
        {
            get => _comparer;
            set
            {
                if (_comparer == value)
                    return;
                _comparer = value;
                // Apply comparer and refresh selected values
                _selectedValues = new HashSet<T?>(_selectedValues, _comparer);
                SelectedValues = _selectedValues;
            }
        }

        private Func<T?, string?>? _toStringFunc = x => x?.ToString();
        /// <summary>
        /// Defines how values are displayed in the drop-down list
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.ListBehavior)]
        public Func<T?, string?>? ToStringFunc
        {
            get => _toStringFunc;
            set
            {
                if (_toStringFunc == value)
                    return;
                _toStringFunc = value;
                Converter = new Converter<T?>
                {
                    SetFunc = _toStringFunc ?? (x => x?.ToString()),
                };
            }
        }

        #endregion


        #region Values, Texts & Items

        private HashSet<T?>? _selectedValues;
        /// <summary>
        /// Set of selected values. If MultiSelection is false it will only ever contain a single value. This property is two-way bindable.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Data)]
        public IEnumerable<T?> SelectedValues
        {
            get
            {
                _selectedValues ??= new HashSet<T?>(_comparer);
                return _selectedValues;
            }
            set
            {
                var set = value ?? new HashSet<T>(_comparer);
                if (value == null && _selectedValues == null)
                {
                    return;
                }
                if (value != null && _selectedValues != null && _selectedValues.SetEquals(value))
                {
                    return;
                }
                if (SelectedValues?.Count() == set.Count() && _selectedValues?.All(x => set.Contains(x, _comparer)) == true)
                    return;

                _selectedValues = new HashSet<T?>(set, _comparer);

                SelectedValuesChanged.InvokeAsync(new HashSet<T?>(SelectedValues, _comparer)).CatchAndLog();
                ForceUpdateItems().CatchAndLog();
                //Console.WriteLine("SelectedValues setter ended");
            }
        }

        /// <summary>
        /// Fires when SelectedValues changes.
        /// </summary>
        [Parameter] public EventCallback<IEnumerable<T?>?> SelectedValuesChanged { get; set; }

        /// <summary>
        /// Protected method.
        /// </summary>
        /// <returns></returns>
        protected Task UpdateDataVisualiserTextAsync()
        {
            var textList = new List<string?>();
            if (Items != null && Items.Any())
            {
                if (false) // ItemCollection != null
                {
                    //foreach (var val in SelectedValues)
                    //{
                    //    var collectionValue = ItemCollection.FirstOrDefault(x => x != null && (Comparer != null ? Comparer.Equals(x, val) : x.Equals(val)));
                    //    if (collectionValue != null)
                    //    {
                    //        textList.Add(Converter.Set(collectionValue));
                    //    }
                    //}
                }
                else
                {
                    foreach (var val in SelectedValues)
                    {
                        if (!Strict && !Items.Select(x => x.Value).Contains(val))
                        {
                            textList.Add(ToStringFunc != null ? ToStringFunc(val) : Converter.Set(val));
                            continue;
                        }
                        var item = Items.FirstOrDefault(x => x != null && (x.Value == null ? val == null : Comparer != null ? Comparer.Equals(x.Value, val) : x.Value.Equals(val)));
                        if (item != null)
                        {
                            textList.Add(!string.IsNullOrWhiteSpace(item.Text) ? item.Text : Converter.Set(item.Value));
                        }
                    }
                }
            }

            // when multiselection is true, we return
            // a comma separated list of selected values
            if (MultiSelection)
            {
                if (MultiSelectionTextFunc != null)
                {
                    if (!SelectedValues.Any())
                    {
                        _dataVisualiserText = null;
                        return Task.CompletedTask;
                    }
                    _dataVisualiserText = MultiSelectionTextFunc.Invoke(SelectedValues.ToList());
                    return Task.CompletedTask;
                }
                else
                {
                    _dataVisualiserText = string.Join(Delimiter, textList);
                    return Task.CompletedTask;
                }
            }
            else
            {
                var item = Items?.FirstOrDefault(x => Value == null ? x.Value == null : Comparer != null ? Comparer.Equals(Value, x.Value) : Value.Equals(x.Value));
                _dataVisualiserText = item is null
                    ? Converter.Set(Value)
                    : (!string.IsNullOrWhiteSpace(item.Text) ? item.Text : Converter.Set(item.Value));

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateText"></param>
        /// <param name="updateSearchString"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        protected async Task UpdateComboBoxValueAsync(T? value, bool updateText = true, bool updateSearchString = false, bool force = false)
        {
            await SetValueAsync(value, updateText, force);
            if (updateSearchString)
            {
                _searchString = Converter.Set(Value);
                await _inputReference.SetText(_searchString);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal string? _dataVisualiserText;

        /// <summary>
        /// Gets DataVisualiserText.
        /// </summary>
        /// <returns></returns>
        protected internal string? GetPresenterText()
        {
            return _dataVisualiserText;
        }

        #endregion


        #region Lifecycle Methods

        /// <summary>
        /// 
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            UpdateIcon();
            if (!MultiSelection && Value != null)
            {
                _selectedValues = new HashSet<T?>(_comparer) { Value };
            }
            else if (MultiSelection && SelectedValues != null)
            {
                // TODO: Check this line again
                SetValueAsync(SelectedValues.FirstOrDefault()).CatchAndLog();
            }

        }

        bool _oldShowCheckbox = true;
        bool _oldBordered;
        Dense _oldDense = Dense.Standard;
        /// <summary>
        /// 
        /// </summary>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (_oldShowCheckbox != ShowCheckbox ||
                _oldBordered != Bordered ||
                _oldDense != Dense)
            {
                ForceRenderItems();
            }
            _oldShowCheckbox = ShowCheckbox;
            _oldBordered = Bordered;
            _oldDense = Dense;
            _allSelected = GetAllSelectedState();
        }

        bool _firstRendered = false;
        T? _oldValue;
        bool _oldMultiselection = false;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            UpdateIcon();
            if (MultiSelection != _oldMultiselection)
            {
                if (_firstRendered == true)
                {
                    await SyncMultiselectionValues(MultiSelection);
                }
                ForceRenderItems();
                if (MultiSelection == true)
                {
                    _searchString = null;
                }
                else
                {
                    DeselectAllItems();
                    if (Value is not null)
                        Items.FirstOrDefault(x => x.Value?.Equals(Value) == true).Selected = true;
                }
            }
            if ((Value == null && _oldValue != null) || (Value != null && Value.Equals(_oldValue) == false))
            {
                await ForceUpdateItems();
                if (MultiSelection == false)
                {
                    _searchString = Converter.Set(Value);
                    if (_inputReference != null)
                    {
                        await _inputReference?.SetText(_searchString);
                    }
                }
            }
            await UpdateDataVisualiserTextAsync();
            _oldMultiselection = MultiSelection;
            _oldValue = Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (firstRender)
            {
                var options = new KeyInterceptorOptions(
                    "mud-input-control",
                    [
                        // prevent scrolling page, toggle open/close
                        new(" ", preventDown: "key+none"),
                        // prevent scrolling page, instead highlight previous item
                        new("ArrowUp", preventDown: "key+none"),
                        // prevent scrolling page, instead highlight next item
                        new("ArrowDown", preventDown: "key+none"),
                        new("Home", preventDown: "key+none"),
                        new("End", preventDown: "key+none"),
                        new("Escape"),
                        new("Enter", preventDown: "key+none"),
                        new("NumpadEnter", preventDown: "key+none"),
                        // select all items instead of all page text
                        new("a", preventDown: "key+ctrl"),
                        // select all items instead of all page text
                        new("A", preventDown: "key+ctrl"),
                        // for our users
                        new("/./", subscribeDown: true, subscribeUp: true)
                    ]);

                await KeyInterceptorService.SubscribeAsync(_elementId, options, keyDown: HandleKeyDownAsync, keyUp: HandleKeyUpAsync);

                await UpdateDataVisualiserTextAsync();
                _firstRendered = true;
                StateHasChanged();
            }
            //Console.WriteLine("Select rendered");
            await base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();

            if (IsJSRuntimeAvailable)
            {
                await KeyInterceptorService.UnsubscribeAsync(_elementId);
            }
        }

        #endregion


        #region Events (Key, Focus)

        /// <summary>
        /// Protected keydown event.
        /// </summary>
        /// <param name="obj"></param>
        protected internal async Task HandleKeyDownAsync(KeyboardEventArgs obj)
        {
            if (Disabled || ReadOnly)
                return;

            // Select mode: Jump to item which starts with "key".
            if (!Editable && obj.Key.Length == 1 && obj.Key[0] != 32 && !(obj.CtrlKey || obj.ShiftKey || obj.AltKey || obj.MetaKey))
            {
                _ = ActivateFirstItem(obj.Key);
                _ = OnKeyDown.InvokeAsync(obj);
                return;
            }

            switch (obj.Key)
            {
                case " ":
                    // Only open the ComboBox menu when search string is empty.
                    if (!Editable || _searchString is null || _searchString.Length == 0)
                        _ = ToggleMenu();
                    else
                        // For example: "new jersey"
                        _searchString += " ";
                    break;

                case "Escape":
                    _ = CloseMenu();
                    break;

                case "Home":
                    if (!_isOpen)
                        _ = OpenMenu();
                    _ = ActivateFirstItem();
                    break;
                case "End":
                    if (!_isOpen)
                        _ = OpenMenu();
                    _ = ActivateLastItem();
                    break;
                case "PageUp":
                    if (_isOpen)
                        _ = ActivateAdjacentItem(-3);
                    break;
                case "PageDown":
                    if (_isOpen)
                        _ = ActivateAdjacentItem(3);
                    break;
                case "ArrowUp":
                    if (obj.AltKey)
                        _ = CloseMenu();
                    else
                    {
                        if (!_isOpen)
                            _ = OpenMenu();
                        _ = ActivateAdjacentItem(-1);
                    }
                    break;
                case "ArrowDown":
                    if (obj.AltKey)
                        _ = OpenMenu();
                    else
                    {
                        if (!_isOpen)
                            _ = OpenMenu();
                        _ = ActivateAdjacentItem(1);
                    }
                    break;

                case "Enter":
                case "NumpadEnter":
                    var doSelect = _lastActivatedItem is null || !_lastActivatedItem.Selected;

                    if (_isOpen)
                        _ = ToggleOption(_lastActivatedItem, doSelect);
                    else
                        _ = OpenMenu();

                    if (MultiSelection)
                    {
                        if (_isOpen && !doSelect)
                            _lastActivatedItem?.SetActive(true);
                    }
                    break;

                case "Tab":
                    if (SelectValueOnTab && !MultiSelection)
                        _ = ToggleOption(_lastActivatedItem, true);
                    _ = CloseMenu();
                    break;

                case "a":
                case "A":
                    if (obj.CtrlKey)
                    {
                        if (MultiSelection)
                            _ = SelectAllItems();
                    }
                    break;
            }

            _ = OnKeyDown.InvokeAsync(obj);
        }

        /// <summary>
        /// Protected searchbox keydown event.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected internal async Task SearchBoxHandleKeyDown(KeyboardEventArgs obj)
        {
            if (Disabled || ReadOnly)
                return;

            switch (obj.Key)
            {
                case "Escape":
                case "Home":
                case "End":
                case "PageUp":
                case "PageDown":
                case "ArrowUp":
                case "ArrowDown":
                case "Enter":
                case "NumpadEnter":
                    await HandleKeyDownAsync(obj);
                    return;

                case "Tab":
                    await ActivateFirstItem();
                    await FocusAsync();
                    break;
            }

            await OnKeyDown.InvokeAsync(obj);
        }

        /// <summary>
        /// Protected searchbox keyup event.
        /// </summary>
        /// <param name="obj"></param>
        protected internal async Task SearchBoxHandleKeyUp(KeyboardEventArgs obj)
        {
            ForceRenderItems();
        }

        /// <summary>
        /// Protected keyup event.
        /// </summary>
        /// <param name="obj"></param>
        protected internal async Task HandleKeyUpAsync(KeyboardEventArgs obj)
        {
            ForceRenderItems();
            await OnKeyUp.InvokeAsync(obj);
        }

        /// <summary>
        /// Protected blur event.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected internal async Task HandleOnBlur(FocusEventArgs obj)
        {
            if (!MultiSelection)
            {
                if (Strict)
                {
                    // Check if the user-provided search string is an exact (case-insensitive) match against an item in the collection.
                    var item = Items.FirstOrDefault(x => Converter.Set(x.Value)?.Equals(_searchString, StringComparison.OrdinalIgnoreCase) == true);
                    if (item is not null)
                        await ToggleOption(item, true);

                    // No item equals the user-provided search string.
                    else
                    {
                        // Restore the previous selected item, if any.
                        if (Value is not null)
                        {
                            item = Items.FirstOrDefault(x => x?.Value?.Equals(Value) == true);
                            if (item is not null)
                                await ToggleOption(item, true);

                            // Remove non-matching search string.
                            else
                                await Clear();
                        }

                        // There was no previous selected item. Remove non-matching search string.
                        else
                            await Clear();
                    }
                }
                else
                    await UpdateComboBoxValueAsync(Converter.Get(_searchString), updateText: true, updateSearchString: true);
            }

            await OnBlurredAsync(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        protected internal void HandleInternalValueChanged(string val)
        {
            _searchString = val;
        }

        /// <summary>
        /// Focuses the component.
        /// </summary>
        /// <returns></returns>
        public override ValueTask FocusAsync()
        {
            return _inputReference.FocusAsync();
        }

        /// <summary>
        /// Blur from the component.
        /// </summary>
        /// <returns></returns>
        public override ValueTask BlurAsync()
        {
            return _inputReference.BlurAsync();
        }

        /// <summary>
        /// Focus and select all text.
        /// </summary>
        /// <returns></returns>
        public override ValueTask SelectAsync()
        {
            return _inputReference.SelectAsync();
        }

        /// <summary>
        /// Focus and select partial text regard of given positions.
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public override ValueTask SelectRangeAsync(int pos1, int pos2)
        {
            return _inputReference.SelectRangeAsync(pos1, pos2);
        }

        #endregion


        #region PopoverState

        /// <summary>
        /// Toggles the menu.
        /// </summary>
        /// <returns></returns>
        public Task ToggleMenu()
        {
            if (Disabled || ReadOnly)
                return Task.CompletedTask;

            return _isOpen ? CloseMenu() : OpenMenu();
        }

        /// <summary>
        /// Opens menu.
        /// </summary>
        /// <returns></returns>
        public async Task OpenMenu()
        {
            if (Disabled || ReadOnly)
                return;

            _isOpen = true;
            UpdateIcon();

            // Disable escape propagation: if ComboBox menu is open, only the ComboBox popover should close and underlying components should not handle escape key.
            await KeyInterceptorService.UpdateKeyAsync(_elementId, new KeyOptions("Escape", stopDown: "key+none"));

            _allSelected = GetAllSelectedState();

            _lastActivatedItem = Items.FirstOrDefault(x => x.Value?.Equals(MultiSelection ? SelectedValues.LastOrDefault() : Value) == true);
            if (_lastActivatedItem is not null)
                await ScrollToMiddleAsync(_lastActivatedItem);
            else
                await ActivateFirstItem();

            if (Editable)
            {
                if (MultiSelection)
                {
                    if (SearchBoxAutoFocus)
                    {
                        await Task.Delay(1);
                        await _searchbox.SelectAsync();
                    }
                }
                else
                    await _inputReference.SelectAsync();
            }

            await OnOpen.InvokeAsync();
        }

        /// <summary>
        /// Closes menu.
        /// </summary>
        /// <returns></returns>
        public async Task CloseMenu()
        {
            _isOpen = false;
            UpdateIcon();
            DeactiveAllItems();

            if (MultiSelection && Editable)
                _searchString = null;

            // Enable escape propagation: The ComboBox popover was closed, no underlying components are allowed to handle escape key.
            await KeyInterceptorService.UpdateKeyAsync(_elementId, new KeyOptions("Escape", stopDown: "none"));

            await OnClose.InvokeAsync();
        }

        #endregion


        #region Item Registration & Selection

        /// <summary>
        /// Select or unselect the ComboBoxItem.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected internal async Task ToggleOption(MudComboBoxItem<T>? item, bool selected)
        {
            if (item == null)
            {
                return;
            }

            if (selected == false)
            {
                if (MultiSelection == false && Value?.Equals(item.Value) == true)
                {
                    if (ToggleSelection)
                    {
                        await UpdateComboBoxValueAsync(default, updateText: true, updateSearchString: true);
                        item.Selected = false;
                    }
                }
                else if (MultiSelection == true && SelectedValues?.Contains(item.Value) == true)
                {
                    SelectedValues = SelectedValues.Where(x => x.Equals(item.Value) == false);
                    await SetValueAsync(SelectedValues.LastOrDefault(), false);
                    item.Selected = false;
                    _allSelected = GetAllSelectedState();
                }
            }
            else
            {
                if (MultiSelection == false)
                {
                    DeselectAllItems();
                    await UpdateComboBoxValueAsync(item.Value, updateText: true, updateSearchString: true);
                }
                else if (SelectedValues?.Contains(item.Value) != true)
                {
                    await SetValueAsync(item.Value, false);
                    SelectedValues = SelectedValues.Append(item.Value);
                    await SelectedValuesChanged.InvokeAsync(_selectedValues);
                    _allSelected = GetAllSelectedState();

                    //await Task.Delay(1);
                }
                item.Selected = true;
            }
            DeactiveAllItems();
            _lastActivatedItem = item;
            await UpdateDataVisualiserTextAsync();
            if (MultiSelection == false)
            {
                await CloseMenu();
            }
            else
            {
                //await FocusAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void DeselectAllItems()
        {
            foreach (var item in CollectionsMarshal.AsSpan(Items))
            {
                if (item.Selected)
                    item.Selected = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task ForceUpdate()
        {
            await base.ForceUpdate();
            if (!MultiSelection)
            {
                SelectedValues = new HashSet<T?>(_comparer) { Value };
            }
            else
            {
                await SelectedValuesChanged.InvokeAsync(new HashSet<T?>(SelectedValues, _comparer));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected internal bool? AddItem(MudComboBoxItem<T> item)
        {
            if (item == null)
                return false;
            bool? result = null;
            if (Items.Select(x => x.Value).Contains(item.Value) == false)
            {
                Items.Add(item);
                if (MultiSelection == true && SelectedValues?.Contains(item.Value) == true)
                {
                    item.Selected = true;
                }
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        protected internal void RemoveItem(MudComboBoxItem<T> item) => Items.Remove(item);

        #endregion


        #region Clear

        /// <summary>
        /// Extra handler for clearing selection.
        /// </summary>
        protected async Task ClearButtonClickHandlerAsync(MouseEventArgs e)
        {
            await UpdateComboBoxValueAsync(default);
            _searchString = null;
            await SetTextAsync(default, false);
            _selectedValues?.Clear();
            DeselectAllItems();
            await BeginValidateAsync();
            await ForceUpdateItems();
            StateHasChanged();
            await SelectedValuesChanged.InvokeAsync(new HashSet<T?>(SelectedValues, _comparer));
            await OnClearButtonClick.InvokeAsync(e);

            if (OpenMenuAfterClear)
                await OpenMenu();
        }

        /// <summary>
        /// Clear the selection
        /// </summary>
        public async Task Clear()
        {
            await SetValueAsync(default, false);
            _searchString = null;
            await SetTextAsync(default, false);
            _selectedValues?.Clear();
            await BeginValidateAsync();
            StateHasChanged();
            await SelectedValuesChanged.InvokeAsync(new HashSet<T?>(SelectedValues, _comparer));

        }

        /// <summary>
        /// 
        /// </summary>
        protected override async Task ResetValueAsync()
        {
            await base.ResetValueAsync();
            SelectedValues = new HashSet<T?>(_comparer);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected void UpdateIcon()
        {
            _currentIcon = !string.IsNullOrWhiteSpace(AdornmentIcon) ? AdornmentIcon : _isOpen ? CloseIcon : OpenIcon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool HasValue(T? value) => MultiSelection ? SelectedValues?.Any() == true : base.HasValue(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chip"></param>
        /// <returns></returns>
        protected async Task ChipClosed(MudChip<T> chip)
        {
            if (chip is null || SelectedValues is null)
                return;

            //SelectedValues = SelectedValues.Where(x => Converter.Set(x)?.ToString() != chip.Value?.ToString());
            SelectedValues = SelectedValues.Where(x => x?.Equals(chip.Value) == false);
            await SelectedValuesChanged.InvokeAsync(SelectedValues);
        }

        private bool? _allSelected;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected async Task SelectAllItems()
        {
            if (_allSelected == null || _allSelected == false)
            {
                SelectedValues = new List<T>();
                foreach (var item in Items.Where(x => x.Eligible))
                {
                    item.Selected = true;
                    SelectedValues = SelectedValues.Append(item.Value);
                }
                await SelectedValuesChanged.InvokeAsync(SelectedValues);
                await SetValueAsync(SelectedValues.LastOrDefault(), false);
                _allSelected = true;
            }
            else
            {
                foreach (var item in Items.Where(x => x.Selected))
                {
                    item.Selected = false;
                }
                SelectedValues = new HashSet<T?>(_comparer);
                await SelectedValuesChanged.InvokeAsync(SelectedValues);
                _allSelected = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool? GetAllSelectedState()
        {
            if (MultiSelection)
            {
                var count = SelectedValues?.Count();
                if (count == 0)
                    return false;

                if (count == Items.Count)
                    return true;

            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal void ForceRenderItems()
        {
            Items.ForEach((x) => x.ForceRender());
            StateHasChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal async Task ForceUpdateItems()
        {
            Items.ForEach(async (x) => await x.ForceUpdate());
        }

        #region Active (Hilight)

        //protected int GetActiveProperItemIndex()
        //{
        //    var properItems = GetEnabledAndEligibleItems();
        //    if (properItems.Any())
        //    {
        //        if (_lastActivatedItem == null)
        //        {
        //            var a = properItems.FindIndex(x => x.Active == true);
        //            return a;
        //        }
        //        else
        //        {
        //            var a = properItems.FindIndex(x => _lastActivatedItem.Value == null ? x.Value == null : Comparer != null ? Comparer.Equals(_lastActivatedItem.Value, x.Value) : _lastActivatedItem.Value.Equals(x.Value));
        //            return a;
        //        }
        //    }
        //    return -1;
        //}

        /// <summary>
        /// 
        /// </summary>
        protected void DeactiveAllItems()
        {
            if (_lastActivatedItem is not null)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(DeactiveAllItems)}: skipped loop");
                _lastActivatedItem.SetActive(false);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(DeactiveAllItems)}: doing loop");
            foreach (var item in CollectionsMarshal.AsSpan(Items))
            {
                if (item.Active)
                    item.SetActive(false);
            }
        }

        /// <summary>
        /// Highlights the first item.
        /// </summary>
        /// <param name="firstLetter"></param>
        /// <returns></returns>
        public async Task ActivateFirstItem(string? firstLetter = null)
        {
            var item = Items.FirstOrDefault();
            if (item is null || item.Disabled)
                return;

            DeactiveAllItems();

            if (string.IsNullOrWhiteSpace(firstLetter))
            {
                item = GetEnabledAndEligibleItems().FirstOrDefault();
                if (item is not null)
                {
                    item.SetActive(true);
                    await ScrollToMiddleAsync(item);
                }
                _lastActivatedItem = item;
                return;
            }

            if (Editable)
                return;


            // Get a collection of items that start with "firstLetter".
            var items = Items.Where(x => Converter?.Set(x.Value)?.StartsWith(firstLetter, StringComparison.OrdinalIgnoreCase) == true).ToList();
            if (!items.Any())
            {
                if (_lastActivatedItem is not null)
                {
                    _lastActivatedItem.SetActive(true);
                    await ScrollToMiddleAsync(_lastActivatedItem);
                }
                return;
            }

            item = items.FirstOrDefault(x => x.Equals(_lastActivatedItem));
            if (item is null)
                item = items[0];
            else if (item.Equals(items.LastOrDefault()))
                item = items[0];
            else
            {
                var index = items.IndexOf(item) + 1;
                if (index >= 0 && index <= items.Count - 1)
                    item = items[index];
            }

            if (item is not null)
            {
                item.SetActive(true);
                await ScrollToMiddleAsync(item);
                _lastActivatedItem = item;
            }
        }

        /// <summary>
        /// Highlight previous or next items.
        /// </summary>
        /// <param name="changeCount"></param>
        /// <returns></returns>
        public async Task ActivateAdjacentItem(int changeCount)
        {
            if (!(Items.Count > 0))
                return;

            DeactiveAllItems();

            var items = GetEnabledAndEligibleItems();
            if (Editable && items.Count == 0)
                return;

            var indexUpperMax = items.Count - 1;
            var index = items.IndexOf(_lastActivatedItem ?? new());

            if (changeCount < 0)
            {
                // Going backward/up using Arrow Up/Page up keys.

                if (index == 0)
                    index = indexUpperMax;
                else if (index + changeCount < 0)
                    index = 0;
                else
                    index += changeCount;
            }
            else
            {
                // Going forward/down using Arrow Down/Page Down keys.

                if (index == indexUpperMax)
                    index = 0;
                else if (index + changeCount > indexUpperMax)
                    index = indexUpperMax;
                else
                    index += changeCount;
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(ActivateAdjacentItem)}: index: {index} / items.Count: {items.Count}");

            var item = items[index];
            item.SetActive(true);
            await ScrollToMiddleAsync(item);
            _lastActivatedItem = item;
        }

        /// <summary>
        /// Highlights the last item.
        /// </summary>
        /// <returns></returns>
        public async Task ActivateLastItem()
        {
            if (!(Items.Count > 0))
                return;

            DeactiveAllItems();

            var item = GetEnabledAndEligibleItems().LastOrDefault();
            if (item is not null)
            {
                item.SetActive(true);
                await ScrollToMiddleAsync(item);
            }
            _lastActivatedItem = item;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal List<MudComboBoxItem<T>> GetEnabledAndEligibleItems() => Items.Where(x => !x.Disabled && x.Eligible).ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool HasEligibleItems()
        {
            foreach (var item in CollectionsMarshal.AsSpan(Items))
            {
                if (item.Eligible)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal string? GetSearchString() => _searchString;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal Typo GetTypo() => Dense == Dense.Slim || Dense == Dense.Superslim ? Typo.body2 : Typo.body1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected internal ValueTask ScrollToMiddleAsync(MudComboBoxItem<T> item) => item is not null ? ScrollManagerExtended.ScrollToMiddleAsync(_popoverId, item.ItemId) : ValueTask.CompletedTask;

        /// <summary>
        /// 
        /// </summary>
        protected internal Color EffectiveCheckBoxCheckedColor => CheckBoxCheckedColor ?? Color;

        /// <summary>
        /// 
        /// </summary>
        protected internal Color EffectiveCheckBoxUnCheckedColor => CheckBoxUnCheckedColor ?? Color;
    }
}
