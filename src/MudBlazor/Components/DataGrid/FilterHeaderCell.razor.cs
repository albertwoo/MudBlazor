// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor.Interfaces;
using MudBlazor.Utilities;

namespace MudBlazor
{
    public partial class FilterHeaderCell<T> : MudComponentBase
    {
        [CascadingParameter] public MudDataGrid<T> DataGrid { get; set; }

        [Parameter] public Column<T> Column { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }

        private string _classname =>
            new CssBuilder(Column?.HeaderClass)
                .AddClass(Column?.headerClassname)
                .AddClass(Class)
                .AddClass("filter-header-cell")
                .Build();

        private string _style =>
            new StyleBuilder()
                .AddStyle(Column?.HeaderStyle)
                .AddStyle(Style)
                .Build();

        private string _valueString => Column.FilterContext.FilterDefinition.Value?.ToString();
        private double? _valueNumber => (double?)Column.FilterContext.FilterDefinition.Value;
        private Enum _valueEnum => (Enum)Column.FilterContext.FilterDefinition.Value;
        private bool? _valueBool => (bool?)Column.FilterContext.FilterDefinition.Value;
        private DateTime? _valueDate => (DateTime?)Column.FilterContext.FilterDefinition.Value;
        private TimeSpan? _valueTime => _valueDate?.TimeOfDay;

        #region Computed Properties and Functions

        private Type dataType
        {
            get
            {
                return Column?.PropertyType;
            }
        }

        private string[] operators
        {
            get
            {
                return FilterOperator.GetOperatorByDataType(dataType);
            }
        }

        private string _operator => Column.FilterContext.FilterDefinition.Operator ?? operators.FirstOrDefault();

        private string chosenOperatorStyle(string o)
        {
            return o == _operator ? "color:var(--mud-palette-primary-text);background-color:var(--mud-palette-primary)" : "";
        }

        private bool isNumber
        {
            get
            {
                return TypeIdentifier.IsNumber(dataType);
            }
        }

        private bool isEnum
        {
            get
            {
                return TypeIdentifier.IsEnum(dataType);
            }
        }

        #endregion

        #region Events

        private async Task ChangeOperatorAsync(string o)
        {
            Column.FilterContext.FilterDefinition.Operator = o;
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }

        internal async Task StringValueChangedAsync(string value)
        {
            Column.FilterContext.FilterDefinition.Operator = _operator;
            Column.FilterContext.FilterDefinition.Value = value;
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }

        internal async Task NumberValueChangedAsync(double? value)
        {
            Column.FilterContext.FilterDefinition.Operator = _operator;
            Column.FilterContext.FilterDefinition.Value = value;
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }

        internal async Task EnumValueChangedAsync(Enum value)
        {
            Column.FilterContext.FilterDefinition.Operator = _operator;
            Column.FilterContext.FilterDefinition.Value = value;
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }

        internal async Task BoolValueChangedAsync(bool? value)
        {
            Column.FilterContext.FilterDefinition.Operator = _operator;
            Column.FilterContext.FilterDefinition.Value = value;
            await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
        }

        internal async Task DateValueChangedAsync(DateTime? value)
        {
            if (value != null)
            {
                var date = value.Value.Date;

                // get the time component and add it to the date.
                if (_valueTime != null)
                {
                    date.Add(_valueTime.Value);
                }

                Column.FilterContext.FilterDefinition.Operator = _operator;
                Column.FilterContext.FilterDefinition.Value = date;
                await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
            }
            else
            {
                Column.FilterContext.FilterDefinition.Operator = _operator;
                Column.FilterContext.FilterDefinition.Value = value;
                await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
            }
        }

        internal async Task TimeValueChangedAsync(TimeSpan? value)
        {
            if (_valueDate != null)
            {
                var date = _valueDate.Value.Date;

                // get the time component and add it to the date.
                if (_valueTime != null)
                {
                    date = date.Add(_valueTime.Value);
                }

                Column.FilterContext.FilterDefinition.Operator = _operator;
                Column.FilterContext.FilterDefinition.Value = date;
                await ApplyFilterAsync(Column.FilterContext.FilterDefinition);
            }
        }

        internal async Task ApplyFilterAsync(IFilterDefinition<T> filterDefinition)
        {
            if (!DataGrid.FilterDefinitions.Any(x => x.Id == filterDefinition.Id))
                DataGrid.FilterDefinitions.Add(filterDefinition);
            if (DataGrid.ServerData is not null) await DataGrid.ReloadServerData();

            DataGrid.GroupItems();
            ((IMudStateHasChanged)DataGrid).StateHasChanged();
        }

        private async Task ClearFilterAsync()
        {
            await ClearFilterAsync(Column.FilterContext.FilterDefinition);
        }

        internal async Task ClearFilterAsync(IFilterDefinition<T> filterDefinition)
        {
            filterDefinition.Value = null;
            await DataGrid.RemoveFilterAsync(filterDefinition.Id);
        }

        #endregion
    }
}
