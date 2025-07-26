using System.Collections.ObjectModel;
using System.Linq;
using Aurelio.Public.Classes.Entries;
using Aurelio.Public.Classes.Enum;
using Aurelio.Public.Module.Service;
using Aurelio.Public.ViewModels;
using Avalonia.Input;
using DynamicData;
using Ursa.Controls;

namespace Aurelio.Views.Overlay;

public partial class AggregateSearchDialog : PageMixModelBase
{
    private string _aggregateSearchFilter = "";
    public ObservableCollection<AggregateSearchEntry> FilteredAggregateSearchEntries { get; } = [];

    public string AggregateSearchFilter
    {
        get => _aggregateSearchFilter;
        set
        {
            SetField(ref _aggregateSearchFilter, value);
            Filter();
        }
    }

    private void Filter()
    {
        FilteredAggregateSearchEntries.Clear();

        FilteredAggregateSearchEntries.AddRange(Data.AggregateSearchEntries.Where(item =>
                item.Title.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase) ||
                item.Summary.Contains(AggregateSearchFilter, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Order).ThenBy(x => x.Title));
    }

    public AggregateSearchDialog()
    {
        InitializeComponent();
        Init();
    }

    private void Init()
    {
        DataContext = this;
        Data.AggregateSearchEntries.CollectionChanged += (_, _) => Filter();
        Width = 870;
        Height = 550;
        PointerMoved += (_, _) => { AggregateSearchBox.Focus(); };
        ComboBox.SelectionChanged += (_, _) => { Filter(); };
        AggregateSearchListBox.SelectionChanged += (_, _) =>
        {
            if (AggregateSearchListBox.SelectedItem is not AggregateSearchEntry entry) return;
            var host = TopLevel.GetTopLevel(this);
            if (host is not DialogWindow window) return;
            AggregateSearch.Execute(entry, window.Owner!);
            window.Close();
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is not Key.Escape) return;
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        };
        Loaded += (_, _) => { AggregateSearchBox.Focus(); };
        Filter();
    }
}