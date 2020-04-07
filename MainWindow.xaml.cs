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
using System.Windows.Documents;
using Npoi.Mapper;

namespace Lab2
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        //private static readonly string directory = Environment.CurrentDirectory;

        private const string link = "https://bdu.fstec.ru/files/documents/thrlist.xlsx";

        private const string fileName = "thrlist.xlsx";

        private const int NotesPerPage = 15;

        private static readonly WebClient webClient = new WebClient();

        public string Status => status ? "успешно" : "ошибка";

        private bool status;

        public ObservableCollection<Note> Notes => new ObservableCollection<Note>(notes.Skip(NotesPerPage * (CurrentPageNumber - 1)).Take(NotesPerPage));

        private List<Note> notes;

        private List<Note> _notes;

        private int PagesCount => notes.Count / NotesPerPage + 1;

        public int CurrentPageNumber { get; private set; }

        public List<Note> Added => added;

        private List<Note> added;

        public List<DoubleNote> Changed => changed;

        private List<DoubleNote> changed;

        public List<Note> Deleted => deleted;

        private List<Note> deleted;

        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists(fileName))
            {
                MessageBox.Show("База данных отсутсвует. Будет загружена");
                try
                {
                    webClient.DownloadFile(new Uri(link), fileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка");
                    Close();
                }
            }

            DataContext = this;
            Load();
        }

        private void Load()
        {
            notes = new Mapper(fileName).Take<Note>().Select(x => x.Value).Skip(1).ToList();
            CurrentPageNumber = 1;
            PrevButton.IsEnabled = false;
            NextButton.IsEnabled = PagesCount != 1;
            OnPropertyChanged("Notes");
            OnPropertyChanged("CurrentPageNumber");
        }

        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            status = true;
            //webClient.DownloadFile(new Uri(link), fileName);
            _notes = new Mapper(fileName).Take<Note>().Select(x => x.Value).Skip(1).ToList();
            added = _notes.Where(_note => !notes.Select(note => note.Id).Contains(_note.Id)).ToList();
            changed = notes.Join(_notes, note => note.Id, _note => _note.Id, (note, _note) => new DoubleNote{OldNote = note, NewNote = _note}).Where(x => x.OldNote != x.NewNote).ToList();
            deleted = notes.Where(note => !_notes.Select(_note => _note.Id).Contains(note.Id)).ToList();
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
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}