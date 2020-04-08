using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Npoi.Mapper;

namespace Lab2
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private const string link = "https://bdu.fstec.ru/files/documents/thrlist.xlsx";

        private const string fileName = "thrlist";

        private const int NotesPerPage = 15;

        private static readonly WebClient webClient = new WebClient();

        public string Status => status ? "успешно" : "ошибка";

        private bool status;

        public ObservableCollection<Note> Notes => new ObservableCollection<Note>(notes.Skip(NotesPerPage * (CurrentPageNumber - 1)).Take(NotesPerPage));

        private List<Note> notes = new List<Note>();

        private List<Note> _notes = new List<Note>();

        private int PagesCount => notes.Count == 0 ? 1 : (notes.Count - (notes.Count % NotesPerPage == 0 ? 1 : 0)) / NotesPerPage + 1;

        public int CurrentPageNumber { get; private set; }

        public List<Note> Added => added;

        private List<Note> added = new List<Note>();

        public List<DoubleNote> Changed => changed;

        private List<DoubleNote> changed = new List<DoubleNote>();

        public List<Note> Deleted => deleted;

        private List<Note> deleted = new List<Note>();

        private readonly Note mainNote;

        public MainWindow()
        {
            InitializeComponent();
            mainNote = new Note
            {
                Id = "Идентификатор УБИ",
                Name = "Наименование УБИ",
                Description = "Описание",
                Source = "Источник угрозы (характеристика и потенциал нарушителя)",
                Target = "Объект воздействия",
                ConfidentialityViolation = "Нарушение конфиденциальности",
                IntegrityViolation = "Нарушение целостности",
                AvailabilityViolation = "Нарушение доступности",
                FirstInclusion = "Дата включения угрозы в БДУ",
                LastChange = "Дата последнего изменения данных"
            };
            if (File.Exists(fileName + ".json"))
            {
                try
                {
                    notes = JsonConvert.DeserializeObject<List<Note>>(File.ReadAllText(fileName + ".json"));
                }
                catch (JsonSerializationException)
                {
                    f();
                }
            }
            else
            {
                f();
            }

            DataContext = this;
            Load();
        }

        private void f()
        {
            if (!File.Exists(fileName + ".xlsx"))
            {
                MessageBox.Show("База данных отсутсвует. Будет загружена");
                try
                {
                    webClient.DownloadFile(new Uri(link), fileName + ".xlsx");
                }
                catch (WebException e)
                {
                    MessageBox.Show(e.Message);
                    Close();
                }
            }

            notes = new Mapper(fileName + ".xlsx").Take<Note>().Select(x => x.Value).ToList();
            if (IsCorrectTable(ref notes))
                return;
            MessageBox.Show("Неверный формат таблицы");
            Close();
        }

        private void Load()
        {
            CurrentPageNumber = 1;
            PrevButton.IsEnabled = false;
            NextButton.IsEnabled = PagesCount != 1;
            OnPropertyChanged("Notes");
            OnPropertyChanged("CurrentPageNumber");
        }

        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //webClient.DownloadFile(new Uri(link), fileName + ".xlsx");
                _notes = new Mapper(fileName + ".xlsx").Take<Note>().Select(x => x.Value).ToList();
                if (!IsCorrectTable(ref _notes))
                    throw new WebException();
                added = _notes.Where(_note => !notes.Select(note => note.Id).Contains(_note.Id)).ToList();
                changed = notes.Join(_notes, note => note.Id, _note => _note.Id, (note, _note) => new DoubleNote {OldNote = note, NewNote = _note})
                    .Where(x => x.OldNote != x.NewNote)
                    .ToList();
                deleted = notes.Where(note => !_notes.Select(_note => _note.Id).Contains(note.Id)).ToList();
                status = true;
                notes = _notes;
            }
            catch (WebException)
            {
                status = false;
            }

            OnPropertyChanged("Status");
            OnPropertyChanged("Added");
            OnPropertyChanged("Changed");
            OnPropertyChanged("Deleted");
            MainGrid.Visibility = Visibility.Hidden;
            ReportGrid.Visibility = Visibility.Visible;
        }

        private void PrevNoteClick(object sender, RoutedEventArgs e)
        {
            if (--CurrentPageNumber == 1)
                PrevButton.IsEnabled = false;
            NextButton.IsEnabled = true;
            OnPropertyChanged("Notes");
            OnPropertyChanged("CurrentPageNumber");
        }

        private void NextNoteClick(object sender, RoutedEventArgs e)
        {
            if (++CurrentPageNumber == PagesCount)
                NextButton.IsEnabled = false;
            PrevButton.IsEnabled = true;
            OnPropertyChanged("Notes");
            OnPropertyChanged("CurrentPageNumber");
        }

        private void CloseReportClick(object sender, RoutedEventArgs e)
        {
            ReportGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
            Load();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            using (var file = new StreamWriter(fileName + ".json"))
            {
                file.Write(JsonConvert.SerializeObject(notes));
            }
        }

        private static bool IsNumber(string s)
        {
            return !String.IsNullOrEmpty(s) && s.Count(Char.IsDigit) == s.Length;
        }

        private bool IsCorrectTable(ref List<Note> __notes)
        {
            if (__notes.Count == 0 || __notes[0] != mainNote)
                return false;
            __notes = __notes.Where(note => IsNumber(note.Id)).ToList();
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var note = (Note) ((DataGridRow) sender).DataContext;
            MessageBox.Show("Идентификатор УБИ: " + note.Id + Environment.NewLine + Environment.NewLine + "Наименование УБИ: " + note.Name + Environment.NewLine +
                            Environment.NewLine + "Описание: " + note.Description + Environment.NewLine + Environment.NewLine +
                            "Источник угрозы (характеристика и потенциал нарушителя): " + note.Source + Environment.NewLine + Environment.NewLine + "Объект воздействия: " +
                            note.Target + Environment.NewLine + Environment.NewLine + "Нарушение конфиденциальности: " + (note.ConfidentialityViolation == "1" ? "Да" : "Нет") +
                            Environment.NewLine + Environment.NewLine + "Нарушение целостности: " + (note.IntegrityViolation == "1" ? "Да" : "Нет") + Environment.NewLine +
                            Environment.NewLine + "Нарушение доступности: " + (note.AvailabilityViolation == "1" ? "Да" : "Нет"));
        }
    }
}