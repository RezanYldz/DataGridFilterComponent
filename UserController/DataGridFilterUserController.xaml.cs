using DataGridFilterComponent.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace DataGridFilterComponent.UserController
{
    /// <summary>
    /// Interaction logic for DataGridFilterUserController.xaml
    /// </summary>
    public partial class DataGridFilterUserController : UserControl
    {
        private DataTable _dataSource;

        #region Filter Variables
        Dictionary<string, ObservableCollection<FilterObjModel>> filters = new();
        private DataTable dataTableFilterList;
        private LastOpenedPopupModel lastOpenedPopup;
        #endregion
        public DataGridFilterUserController(DataTable dataTable)
        {
            InitializeComponent();
            _dataSource = dataTable;

            dataTableFilterList = _dataSource;

            FillDataTable();
        }
        private void FillDataTable()
        {
            DataFilterComponent.ItemsSource = dataTableFilterList?.DefaultView;
        }

        #region Filter Codes
        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            filters.Clear();
            dataTableFilterList = _dataSource;

            FillDataTable();
        }
        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e.OriginalSource is Button currentButton)
                    if (currentButton.Name == "btnFilter")
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        tbFilter.Text = String.Empty;
                        string colName = string.Empty;

                        popFilter.PlacementTarget = currentButton;

                        Grid gr = currentButton.Parent as Grid;

                        foreach (var ch in gr.Children)
                        {
                            if (ch is TextBlock tb)
                            {
                                colName = tb.Text;
                                break;
                            }
                        }

                        if (lastOpenedPopup != null &&
                            lastOpenedPopup.PopupName == colName &&
                            (DateTime.Now - lastOpenedPopup.ClosingTime).TotalMilliseconds < 500)
                        {
                            Mouse.OverrideCursor = Cursors.Arrow;
                            return;
                        }

                        FillFilterKeyList(colName);

                        popFilter.Uid = colName;
                        popFilter.IsOpen = true;
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                MessageBox.Show(ex.Message);
            }
        }
        private void FillFilterKeyList(string colName)
        {
            ObservableCollection<FilterObjModel> filterCurrent = new();
            ObservableCollection<FilterObjModel> newFilterCurrent = new();

            foreach (DataRow row in _dataSource.Rows)
            {
                if (row[colName] != null)
                {
                    string value = row[colName]?.ToString();
                    if (value != null)
                    {
                        FilterObjModel filter = filterCurrent.FirstOrDefault(x => x.PropertyValue == value);
                        if (newFilterCurrent.FirstOrDefault(x => x.PropertyValue == value) is null)
                            if (filter is null)
                            {
                                bool _isChecked = true;
                                if (dataTableFilterList.AsEnumerable().FirstOrDefault(e1 => e1[colName].ToString() == value) == null)
                                    _isChecked = false;
                                newFilterCurrent.Add(new(){ IsChecked = _isChecked, PropertyName = colName, PropertyValue = value });
                            }
                            else
                            {
                                if (dataTableFilterList.AsEnumerable().FirstOrDefault(e1 => e1[colName].ToString() == value) != null)
                                    filter.IsChecked = true;

                                newFilterCurrent.Add(new() { IsChecked = filter.IsChecked, PropertyName = colName, PropertyValue = value });
                            }
                    }
                }
            }

            if (!filters.ContainsKey(colName))
                filters.Add(colName, newFilterCurrent);
            else
                filters[colName] = newFilterCurrent;

            lbFilter.ItemsSource = newFilterCurrent;
        }

        private void btnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                dataTableFilterList = null;

                try
                {
                    var filter = filters.FirstOrDefault(e1 => e1.Key == popFilter.Uid.ToString()).Value;
                    dataTableFilterList = _dataSource?.AsEnumerable()
                        .Where(e1 =>
                        filter.Where(y => y.IsChecked).Select(y => y.PropertyValue).Contains(e1[popFilter.Uid.ToString()]?.ToString()))
                        .CopyToDataTable();
                }
                catch { }

                FillDataTable();

                popFilter.IsOpen = false;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                MessageBox.Show(ex.Message);
            }
        }

        private void tbFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string colName = popFilter.Uid;

            if (string.IsNullOrEmpty(tbFilter.Text))
            {
                FillFilterKeyList(colName);
                return;
            }

            ObservableCollection<FilterObjModel> filterCurrent;

            if (filters.ContainsKey(colName))
            {
                filters.TryGetValue(colName, out filterCurrent);

                IEnumerable<FilterObjModel> filteredFilters = filterCurrent.Where(y => y.PropertyValue.IndexOf(tbFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);

                lbFilter.ItemsSource = filteredFilters;

                if (!filters.ContainsKey(colName))
                    filters.Add(colName, new(filteredFilters));
                else
                    filters[colName] = new(filteredFilters);
            }
        }

        private void popFilter_Closed(object sender, EventArgs e)
        {
            lastOpenedPopup = new()
            {
                PopupName = popFilter.Uid,
                ClosingTime = DateTime.Now
            };
        }
        #endregion

        private void DataFilterComponent_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = new DataGridColumnHeader() { Content = e.PropertyName }; ;
            e.Column.Header = header;
            header.Width = double.NaN;

            try
            {
                if (filters.ContainsKey(e.PropertyName))
                    e.Column.HeaderStyle = FindResource("filteredTemplate") as Style;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
