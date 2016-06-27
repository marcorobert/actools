﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Presentation;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace FirstFloor.ModernUI.Windows.Controls {
    public class TagsList : Control {
        private RelayCommand _closeCommand;

        public RelayCommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(o => {
            ItemsSource.Remove(o as string);
        }));

        private RelayCommand _changeCommand;

        public RelayCommand ChangeCommand => _changeCommand ?? (_changeCommand = new RelayCommand(o => {
            var target = (TextBox)o;
            var originalValue = target.DataContext as string;
            var newValue = target.Text.Trim();
            if (string.IsNullOrEmpty(newValue)) {
                ItemsSource.Remove(originalValue);
            } else {
                ItemsSource[ItemsSource.IndexOf(originalValue)] = target.Text;
            }
        }));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<string>),
            typeof(TagsList));

        public static readonly DependencyProperty SuggestionsSourceProperty = DependencyProperty.Register("SuggestionsSource", typeof(CollectionView),
            typeof(TagsList));

        public static readonly DependencyProperty ItemContextMenuProperty = DependencyProperty.Register(nameof(ItemContextMenu), typeof(ContextMenu),
                typeof(TagsList));

        public ContextMenu ItemContextMenu {
            get { return (ContextMenu)GetValue(ItemContextMenuProperty); }
            set { SetValue(ItemContextMenuProperty, value); }
        }

        public TagsList() {
            DefaultStyleKey = typeof(TagsList);
        }

        public ObservableCollection<string> ItemsSource {
            get { return (ObservableCollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public CollectionView SuggestionsSource {
            get { return (CollectionView)GetValue(SuggestionsSourceProperty); }
            set { SetValue(SuggestionsSourceProperty, value); }
        }
        
        private ComboBox _previousTextBox;

        public override void OnApplyTemplate() {
            if (_previousTextBox != null) {
                _previousTextBox.PreviewKeyDown -= TextBox_KeyDown;
                _previousTextBox.LostFocus -= TextBox_LostFocus;
                _previousTextBox = null;
            }

            base.OnApplyTemplate();

            var textBox = Template.FindName("PART_NewTagTextBox", this) as ComboBox;
            if (textBox == null) return;

            textBox.PreviewKeyDown += TextBox_KeyDown;
            textBox.LostFocus += TextBox_LostFocus;
            _previousTextBox = textBox;
        }

        private void AddNewTag(string tag) {
            var tagLower = tag.ToLower();
            if (ItemsSource.FirstOrDefault(x => x.ToLower() == tagLower) != null) return;
            ItemsSource.Add(tag);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter && e.Key != Key.Tab) return;

            var textBox = sender as ComboBox;
            if (string.IsNullOrWhiteSpace(textBox?.Text)) return;

            AddNewTag(textBox.Text.Trim());
            textBox.Text = "";

            if (e.Key == Key.Tab) {
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            var textBox = sender as ComboBox;
            if (string.IsNullOrWhiteSpace(textBox?.Text)) return;

            AddNewTag(textBox.Text.Trim());
            textBox.Text = "";
        }

        public void RemoveTag(string value) {
            ItemsSource.Remove(value);
        }

        public void ReplaceTag(string oldValue, string newValue) {
            if (ItemsSource.Contains(oldValue)) {
                ItemsSource[ItemsSource.IndexOf(oldValue)] = newValue;
            } else {
                ItemsSource.Add(newValue);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool),
                typeof(TagsList));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
    }

    public class TagsListDataTemplateSelector : DataTemplateSelector {
        public DataTemplate TagDataTemplate { get; set; }

        public DataTemplate NewTagDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            return item is string ? TagDataTemplate : NewTagDataTemplate;
        }
    }
}
