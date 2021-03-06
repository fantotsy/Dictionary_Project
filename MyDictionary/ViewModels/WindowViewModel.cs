﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Threading;
using System.Windows;
using MyDictionary.MVVMSupport;
using MyDictionary;

namespace MyDictionary.ViewModels
{
    public class WindowViewModel : ObservableObject, IDisposable
    {
        #region Global Components

        //Контекст данных.
        private DictionaryDBEntities dataContext;

        //Поле таблицы Eng_Ukr базы данных.
        public static ObservableCollection<Eng_Ukr> Dictionary { get; set; }

        //Реализация метода Dispose для освобождения ресурсов контекста при удалении WindowViewModel.
        public void Dispose()
        {
            dataContext.Dispose();
        }

        //Конструктор класса WindowViewModel. Устанавливает начальные показатели.
        public WindowViewModel()
        {
            dataContext = new DictionaryDBEntities();
            Dictionary = new ObservableCollection<Eng_Ukr>(dataContext.Eng_Ukr);
            Reset();
        }

        //Восстановление всех начальных показателей, кроме привязки к базе данных.
        public void Reset()
        {
            //Установка видимости и доступности объектов.
            SettingsForTestVisibility = "Hidden";
            SettingsForEveryTestVisibility = "Hidden";
            TestControlsVisibility = "Hidden";
            EveryTestControlsVisibility = "Hidden";
            SelectionOfTestVisibility = "Visible";
            SettingsForTranslatorTestVisibility = "Hidden";
            TranslatorTestControlsVisibility = "Hidden";
            IsStartTestButtonEnabled = false;
            IsDeleteButtonEnabled = false;
            WordType = "Not Given";
            NumberOfWords = Dictionary.Count();
            InputEnglishWordBorderColor = "#FFFFFFFF";
            InputTranslationWordBorderColor = "#FFFFFFFF";
            TranslatorAnswerColor = "#FFFFFFFF";
            IsButtonAddWordEnabled = true;
            AreKnowDontKnowButtonsEnabled = false;
            CleverSelectionChecked = false;
            AscendingChecked = false;
            DescendingChecked = false;
            SimpleRandomChecked = false;
            EngUkrTranslationChecked = false;
            UkrEngTranslationChecked = false;
            IsAnswerButtonEnabled = false;
            IsSaveButtonEnabled = true;

            //Привязка команд.
            Add = new DelegateCommand(AddToDictionary);
            Delete = new DelegateCommand(DeleteFromDataBase);
            Save = new DelegateCommand(SaveChanges);
            StartTest = new DelegateCommand(StartDoingTest);
            Know = new DelegateCommand(RightAnswer);
            DoNotKnow = new DelegateCommand(WrongAnswer);
            Next = new DelegateCommand(NextWord);
            End = new DelegateCommand(EndTest);
            YesNotTestSelect = new DelegateCommand(SelectingYesNotTest);
            TranslatorTestSelect = new DelegateCommand(SelectingTranslatorTest);
            TranslatorAnswerConfirmation = new DelegateCommand(TranslatorAnswerConfirm);
        }

        #endregion

        #region MainMenu

        #region Properties

        //Свойство, связанное с выбранным словом.
        private Eng_Ukr selectedValue;
        public Eng_Ukr SelectedValue
        {
            get { return selectedValue; }
            set
            {
                selectedValue = value;
                OnPropertyChanged("SelectedValue");

                Total = (selectedValue.Correct + selectedValue.Incorrect).ToString();
                Correct = selectedValue.Correct.ToString();
                Incorrect = selectedValue.Incorrect.ToString();

                if (selectedValue.English != null)
                {
                    IsDeleteButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с общим количеством попадания слова в тестах.
        private string total;
        public string Total
        {
            get { return total; }
            set
            {
                total = "Total: " + value;
                OnPropertyChanged("Total");
            }
        }

        //Свойство, связанное с количеством правильных ответов на слово в тестах.
        private string correct;
        public string Correct
        {
            get { return correct; }
            set
            {
                correct = "Correct: " + value;
                OnPropertyChanged("Correct");
            }
        }

        //Свойство, связанное с количеством неправильных ответов на слово в тестах.
        private string incorrect;
        public string Incorrect
        {
            get { return incorrect; }
            set
            {
                incorrect = "Incorrect: " + value;
                OnPropertyChanged("Incorrect");
            }
        }

        //Свойство, связанное с ProgressBar.
        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        //Свойство, связанное с количеством слов в словаре (Label).
        private int numberOfWords;
        public int NumberOfWords
        {
            get { return numberOfWords; }
            set
            {
                numberOfWords = value;
                OnPropertyChanged("NumberOfWords");
            }
        }

        //Свойство, связанное с доступностью для нажатия кнопки "Delete".
        private bool isDeleteButtonEnabled;
        public bool IsDeleteButtonEnabled
        {
            get { return isDeleteButtonEnabled; }
            set
            {
                isDeleteButtonEnabled = value;
                OnPropertyChanged("IsDeleteButtonEnabled");
            }
        }

        //Свойство, связанное с доступностью для нажатия кнопки "Save".
        private bool isSaveButtonEnabled;
        public bool IsSaveButtonEnabled
        {
            get { return isSaveButtonEnabled; }
            set
            {
                isSaveButtonEnabled = value;
                OnPropertyChanged("IsSaveButtonEnabled");
            }
        }

        #endregion

        #region Commands

        //Удаление слова из базы данных.
        private void DeleteFromDataBase()
        {
            var selectedWord = (from item in Dictionary
                                where deleteWhiteSpaces(item.English) == deleteWhiteSpaces(selectedValue.English)
                                select item).First();

            Dictionary.Remove(selectedWord);
            dataContext.Eng_Ukr.Remove(selectedWord);
            IsDeleteButtonEnabled = false;
            SaveChanges();
            NumberOfWords--;
        }
        public DelegateCommand Delete { get; set; }

        //Сохранение изменений в базу данных и его визуализация (нажатие на кнопку "Save").
        private void SaveChanges()
        {
            //Сохранение.
            dataContext.SaveChanges();

            //Визуализация.
            IsSaveButtonEnabled = false;
            Random rand = new Random();
            while (Progress < 100)
            {
                Wait(rand.Next(100, 300));
                Progress += 25;
            }
            Wait(500);
            Progress = 0;
            IsSaveButtonEnabled = true;
        }
        public DelegateCommand Save { get; set; }

        #endregion

        #region Additional Components

        public static void Wait(int interval)
        {
            ExecuteWait(() => Thread.Sleep(interval));
        }

        public static void ExecuteWait(Action action)
        {
            var waitFrame = new DispatcherFrame();
            IAsyncResult op = action.BeginInvoke(dummy => waitFrame.Continue = false, null);
            Dispatcher.PushFrame(waitFrame);
            action.EndInvoke(op);
        }

        #endregion

        #endregion

        #region Add Word

        #region Properties

        //Свойство, свзяанное с элементом TextBox для ввода английского слова.
        private string english;
        public string English
        {
            get { return english; }
            set
            {
                english = value;
                OnPropertyChanged("English");

                if (English.Length > 30)
                {
                    InputEnglishWordBorderColor = "#FFFF0000";
                    IsButtonAddWordEnabled = false;
                }
                else
                {
                    InputEnglishWordBorderColor = "#FFFFFFFF";
                    if (InputTranslationWordBorderColor == "#FFFFFFFF")
                    {
                        IsButtonAddWordEnabled = true;
                    }
                }
            }
        }

        //Свойство, связанное с элементом TextBox для ввода слова-перевода.
        private string translation;
        public string Translation
        {
            get { return translation; }
            set
            {
                translation = value;
                OnPropertyChanged("Translation");

                if (Translation.Length > 60)
                {
                    InputTranslationWordBorderColor = "#FFFF0000";
                    IsButtonAddWordEnabled = false;
                }
                else
                {
                    InputTranslationWordBorderColor = "#FFFFFFFF";
                    if (InputEnglishWordBorderColor == "#FFFFFFFF")
                    {
                        IsButtonAddWordEnabled = true;
                    }
                }
            }
        }

        //Свойство, связанное с элементом ComboBox для выбора типа английского слова.
        private string wordType;
        public string WordType
        {
            get { return wordType; }
            set
            {
                wordType = value;
                OnPropertyChanged("WordType");
            }
        }

        //Свойство, связанное с цветом рамки TextBox для английского слова.
        private string inputEnglishWordBorderColor;
        public string InputEnglishWordBorderColor
        {
            get { return inputEnglishWordBorderColor; }
            set
            {
                inputEnglishWordBorderColor = value;
                OnPropertyChanged("InputEnglishWordBorderColor");
            }
        }

        //Свойство, связанное с цветом рамки TextBox для слова-перевода.
        private string inputTranslationWordBorderColor;
        public string InputTranslationWordBorderColor
        {
            get { return inputTranslationWordBorderColor; }
            set
            {
                inputTranslationWordBorderColor = value;
                OnPropertyChanged("InputTranslationWordBorderColor");
            }
        }

        //Свойство, связанное с доступностью для нажатия кнопки "AddWord".
        private bool isButtonAddWordEnabled;
        public bool IsButtonAddWordEnabled
        {
            get { return isButtonAddWordEnabled; }
            set
            {
                isButtonAddWordEnabled = value;
                OnPropertyChanged("IsButtonAddWordEnabled");
            }
        }

        #endregion

        #region Commands

        //Добавления записи в словарь.
        private void AddToDictionary()
        {
            if (English == null || Translation == null || English.Length == 0 || Translation.Length == 0)
            {
                MessageBox.Show("Text boxes 'English' and 'Translation' can not be empty.");
            }
            else
            {
                Eng_Ukr item = new Eng_Ukr
                {
                    English = English,
                    Translation = Translation,
                    Correct = 0,
                    Incorrect = 0,
                    Type = WordType
                };

                bool isSimilarWordFound = false;
                foreach (var word in Dictionary)
                {
                    if (deleteWhiteSpaces(word.English).Equals(item.English) &&
                        deleteWhiteSpaces(word.Translation).Equals(item.Translation))
                    {
                        isSimilarWordFound = true;
                        break;
                    }
                }

                if (isSimilarWordFound)
                {
                    MessageBox.Show("This word is already in your dictionary. You can change it in the table.");
                }
                else
                {
                    English = String.Empty;
                    Translation = String.Empty;
                    WordType = "Not Given";

                    Dictionary.Add(item);
                    dataContext.Eng_Ukr.Add(item);

                    if (Dictionary.Count > 0)
                    {
                        IsDeleteButtonEnabled = true;
                    }
                    NumberOfWords++;
                    SaveChanges();
                }
            }
        }
        public DelegateCommand Add { get; set; }

        #endregion

        #endregion

        #region TEST

        #region Common

        #region Fields

        //Упорядоченные тестируемые слова.
        public ObservableCollection<Eng_Ukr> testedWords = new ObservableCollection<Eng_Ukr>();

        //Итератор по коллекции testedWords.
        private int iterator = 0;

        //Идентификатор типа теста.
        private int testType = 0;

        #endregion

        #region Properties

        //Свойство, связанное с доступностью для нажатия кнопки "Start Test".
        private bool isStartTestButtonEnabled;
        public bool IsStartTestButtonEnabled
        {
            get { return isStartTestButtonEnabled; }
            set
            {
                isStartTestButtonEnabled = value;
                OnPropertyChanged("IsStartTestButtonEnabled");
            }
        }

        //Свойство, связанное с видимостью кнопок выбора типа тестирования.
        private string selectionOfTestVisibility;
        public string SelectionOfTestVisibility
        {
            get { return selectionOfTestVisibility; }
            set
            {
                selectionOfTestVisibility = value;
                OnPropertyChanged("SelectionOfTestVisibility");
            }
        }

        //Свойство, связанное с видимостью кнопки "Start Test" и других общих кнопок.
        private string settingsForEveryTestVisibility;
        public string SettingsForEveryTestVisibility
        {
            get { return settingsForEveryTestVisibility; }
            set
            {
                settingsForEveryTestVisibility = value;
                OnPropertyChanged("SettingsForEveryTestVisibility");
            }
        }

        //Свойство, связанное с видимостью средств тестирования.
        private string everyTestControlsVisibility;
        public string EveryTestControlsVisibility
        {
            get { return everyTestControlsVisibility; }
            set
            {
                everyTestControlsVisibility = value;
                OnPropertyChanged("EveryTestControlsVisibility");
            }
        }

        //Свойство, связанное с Label-вопросом.
        private string englishWord;
        public string EnglishWord
        {
            get { return englishWord; }
            set
            {
                englishWord = deleteWhiteSpaces(value);
                OnPropertyChanged("EnglishWord");
            }
        }

        #endregion

        #region Commands

        //Нажатие кнопки "Start Test".
        private void StartDoingTest()
        {
            Copy(Dictionary, testedWords);
            MixWords();
            if (testType == 1 || (testType == 2 && EngUkrTranslationChecked))
            {
                EnglishWord = testedWords[iterator].English;
            }
            if (testType == 2 && UkrEngTranslationChecked)
            {
                EnglishWord = testedWords[iterator].Translation;
            }

            if (testType == 2)
            {
                TranslatorTestControlsVisibility = "Visible";
            }
            if (testType == 1)
            {
                TestControlsVisibility = "Visible";
            }
            SettingsForEveryTestVisibility = "Hidden";
            EveryTestControlsVisibility = "Visible";
            SettingsForTranslatorTestVisibility = "Hidden";
            SettingsForTestVisibility = "Hidden";
        }
        public DelegateCommand StartTest { get; set; }

        //Нажатие кнопки "Next->".
        private void NextWord()
        {
            TranslationWord = "";

            if (testType == 2)
            {
                TranslatorAnswerColor = "#FFFFFFFF";
                TranslatorAnswer = "";
            }

            UpdateWord();
        }
        public DelegateCommand Next { get; set; }

        //Нажатие кнопки "End Test".
        private void EndTest()
        {
            TestDispose();
        }
        public DelegateCommand End { get; set; }

        #endregion

        #region AdditionalComponents

        //Удаление пробелов после слова, чтобы при отображении слово находилось посередине.
        public string deleteWhiteSpaces(string word)
        {
            int spacesCount = 0;
            for (int i = word.Length - 1; i > 0; i--)
            {
                if (word[i] == ' ')
                {
                    spacesCount++;
                }
                else
                {
                    break;
                }
            }
            return (word.Substring(0, word.Length - spacesCount));
        }

        #endregion

        #endregion

        #region Yes/No

        #region Properties

        //Свойство, связанное с RadioButton "Clever Selection".
        private bool cleverSelectionChecked;
        public bool CleverSelectionChecked
        {
            get { return cleverSelectionChecked; }
            set
            {
                cleverSelectionChecked = value;
                OnPropertyChanged("CleverSelectionChecked");
                if (Dictionary.Count > 0 && value == true)
                {
                    IsStartTestButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с RadioButton "Ascending".
        private bool ascendingChecked;
        public bool AscendingChecked
        {
            get { return ascendingChecked; }
            set
            {
                ascendingChecked = value;
                OnPropertyChanged("AscendingChecked");
                if (Dictionary.Count > 0 && value == true)
                {
                    IsStartTestButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с RadioButton "Descending".
        private bool descendingChecked;
        public bool DescendingChecked
        {
            get { return descendingChecked; }
            set
            {
                descendingChecked = value;
                OnPropertyChanged("DescendingChecked");
                if (Dictionary.Count > 0 && value == true)
                {
                    IsStartTestButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с RadioButton "Simple Random".
        private bool simpleRandomChecked;
        public bool SimpleRandomChecked
        {
            get { return simpleRandomChecked; }
            set
            {
                simpleRandomChecked = value;
                OnPropertyChanged("SimpleRandomChecked");
                if (Dictionary.Count > 0 && value == true)
                {
                    IsStartTestButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с Label-ответом.
        private string translationWord;
        public string TranslationWord
        {
            get { return translationWord; }
            set
            {
                if (value == "")
                {
                    translationWord = value;
                }
                else
                {
                    translationWord = deleteWhiteSpaces(value);
                }

                OnPropertyChanged("TranslationWord");
            }
        }

        //Свойство, связанное с видимостью средств настройки для теста.
        private string settingsForTestVisibility;
        public string SettingsForTestVisibility
        {
            get { return settingsForTestVisibility; }
            set
            {
                settingsForTestVisibility = value;
                OnPropertyChanged("SettingsForTestVisibility");
            }
        }

        //Свойство, связанное с видимостью средств тестирования.
        private string testControlsVisibility;
        public string TestControlsVisibility
        {
            get { return testControlsVisibility; }
            set
            {
                testControlsVisibility = value;
                OnPropertyChanged("TestControlsVisibility");
            }
        }

        //Свойство, связанное с доступностью для нажатия кнопок "Know" и "Don't know".
        private bool areKnowDontKnowButtonsEnabled;
        public bool AreKnowDontKnowButtonsEnabled
        {
            get { return areKnowDontKnowButtonsEnabled; }
            set
            {
                areKnowDontKnowButtonsEnabled = value;
                OnPropertyChanged("AreKnowDontKnowButtonsEnabled");
            }
        }

        #endregion

        #region Commands

        //Выбор Yes/No теста.
        private void SelectingYesNotTest()
        {
            testType = 1;

            SelectionOfTestVisibility = "Hidden";
            SettingsForTestVisibility = "Visible";
            SettingsForEveryTestVisibility = "Visible";

            AreKnowDontKnowButtonsEnabled = true;
        }
        public DelegateCommand YesNotTestSelect { get; set; }

        //Нажатие кнопки "I Know it".
        private void RightAnswer()
        {
            var selected = (from item in Dictionary
                            where item.English == testedWords[iterator].English
                            select item).First();

            selected.Correct++;
            TranslationWord = selected.Translation;

            AreKnowDontKnowButtonsEnabled = false;
        }
        public DelegateCommand Know { get; set; }

        //Нажатие кнопки "Don't know".
        private void WrongAnswer()
        {
            var selected = (from item in Dictionary
                            where item.English == testedWords[iterator].English
                            select item).First();

            selected.Incorrect++;
            TranslationWord = selected.Translation;

            AreKnowDontKnowButtonsEnabled = false;
        }
        public DelegateCommand DoNotKnow { get; set; }

        #endregion

        #endregion

        #region Translator

        #region Properties

        //Свойство, связанное с RadioButton "English-Ukrainian".
        private bool engUkrTranslationChecked;
        public bool EngUkrTranslationChecked
        {
            get { return engUkrTranslationChecked; }
            set
            {
                engUkrTranslationChecked = value;
                OnPropertyChanged("EngUkrTranslationChecked");
                if (Dictionary.Count > 0 && value == true)
                {
                    IsStartTestButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с RadioButton "Ukrainian-English".
        private bool ukrEngTranslationChecked;
        public bool UkrEngTranslationChecked
        {
            get { return ukrEngTranslationChecked; }
            set
            {
                ukrEngTranslationChecked = value;
                OnPropertyChanged("UkrEngTranslationChecked");
                if (Dictionary.Count > 0 && value == true)
                {
                    IsStartTestButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с видимостью средств "Translator" тестирования.
        private string settingsForTranslatorTestVisibility;
        public string SettingsForTranslatorTestVisibility
        {
            get { return settingsForTranslatorTestVisibility; }
            set
            {
                settingsForTranslatorTestVisibility = value;
                OnPropertyChanged("SettingsForTranslatorTestVisibility");
            }
        }

        //Свойство, связанное с видимостью TextBox.
        private string translatorTestControlsVisibility;
        public string TranslatorTestControlsVisibility
        {
            get { return translatorTestControlsVisibility; }
            set
            {
                translatorTestControlsVisibility = value;
                OnPropertyChanged("TranslatorTestControlsVisibility");
            }
        }

        //Свойство, связанное с текстом из TextBox.
        private string translatorAnswer;
        public string TranslatorAnswer
        {
            get { return translatorAnswer; }
            set
            {
                translatorAnswer = value;
                OnPropertyChanged("TranslatorAnswer");
                if (value == "")
                {
                    IsAnswerButtonEnabled = false;
                }
                else
                {
                    IsAnswerButtonEnabled = true;
                }
            }
        }

        //Свойство, связанное с цветом фона TextBox (Green - правильный ответ, Red - иначе).
        private string translatorAnswerColor;
        public string TranslatorAnswerColor
        {
            get { return translatorAnswerColor; }
            set
            {
                translatorAnswerColor = value;
                OnPropertyChanged("TranslatorAnswerColor");
            }
        }

        //Свойство, связанное с доступностью для нажатия кнопки "Answer".
        private bool isAnswerButtonEnabled;
        public bool IsAnswerButtonEnabled
        {
            get { return isAnswerButtonEnabled; }
            set
            {
                isAnswerButtonEnabled = value;
                OnPropertyChanged("IsAnswerButtonEnabled");
            }
        }

        #endregion

        #region Commands

        //Выбор Translator теста.
        private void SelectingTranslatorTest()
        {
            testType = 2;

            SelectionOfTestVisibility = "Hidden";
            SettingsForTranslatorTestVisibility = "Visible";
            SettingsForEveryTestVisibility = "Visible";
        }
        public DelegateCommand TranslatorTestSelect { get; set; }

        //Нажатие кнопки "Answer".
        private void TranslatorAnswerConfirm()
        {
            var selected = (from item in Dictionary
                            where deleteWhiteSpaces(item.English) == deleteWhiteSpaces(testedWords[iterator].English)
                            select item).First();

            var selectedArray = (from item in Dictionary
                                 where deleteWhiteSpaces(item.English) == deleteWhiteSpaces(testedWords[iterator].English)
                                 select item);
            if (EngUkrTranslationChecked)
            {
                List<string> rightAnswers = new List<string>();

                foreach (var item in selectedArray)
                {
                    rightAnswers.AddRange(item.Translation.Split(','));

                    string lastWord = rightAnswers.Last();
                    rightAnswers.RemoveAt(rightAnswers.Count - 1);

                    rightAnswers.Add(deleteWhiteSpaces(lastWord));
                }

                if (rightAnswers.Contains(deleteWhiteSpaces(TranslatorAnswer)))
                {
                    TranslatorAnswerColor = "#FFC3FFC3";
                    selected.Correct++;
                }
                else
                {
                    TranslatorAnswerColor = "#FFFF687D";
                    selected.Incorrect++;
                }
            }
            if (UkrEngTranslationChecked)
            {
                if (deleteWhiteSpaces(testedWords[iterator].English) == deleteWhiteSpaces(TranslatorAnswer))
                {
                    TranslatorAnswerColor = "#FFC3FFC3";
                    selected.Correct++;
                }
                else
                {
                    TranslatorAnswerColor = "#FFFF687D";
                    selected.Incorrect++;
                }
            }

            IsAnswerButtonEnabled = false;
        }
        public DelegateCommand TranslatorAnswerConfirmation { get; set; }

        #endregion

        #endregion

        #region Additional Components

        //Очистить все компоненты теста после его завершения.
        public void TestDispose()
        {
            iterator = 0;
            testedWords.Clear();
            TranslationWord = "";
            Reset();
        }

        //Копировать элементы коллекции collection1 в коллекцию collection2.
        public void Copy(ObservableCollection<Eng_Ukr> collection1, ObservableCollection<Eng_Ukr> collection2)
        {
            foreach (var item in collection1)
            {
                collection2.Add(item);
            }
        }

        //Обновление слова в тесте.
        public void UpdateWord()
        {
            iterator++;
            if (iterator < testedWords.Count)
            {
                if (testType == 1 || (testType == 2 && EngUkrTranslationChecked))
                {
                    EnglishWord = testedWords[iterator].English;
                }
                if (testType == 2 && UkrEngTranslationChecked)
                {
                    EnglishWord = testedWords[iterator].Translation;
                }
                AreKnowDontKnowButtonsEnabled = true;
                IsAnswerButtonEnabled = false;
            }
            else
            {
                MessageBox.Show("Congratulations!");
                TestDispose();
            }
        }

        //Изменение видимости элементов управления в Да/Нет тесте.
        public void ChangeVisibilityInTEST()
        {
            if (SettingsForTestVisibility == "Hidden")
            {
                SettingsForTestVisibility = "Visible";
            }
            else
            {
                SettingsForTestVisibility = "Hidden";
            }
            if (TestControlsVisibility == "Hidden")
            {
                TestControlsVisibility = "Visible";
            }
            else
            {
                TestControlsVisibility = "Hidden";
            }
            if (SettingsForEveryTestVisibility == "Hidden")
            {
                SettingsForEveryTestVisibility = "Visible";
            }
            else
            {
                SettingsForEveryTestVisibility = "Hidden";
            }

            if (EveryTestControlsVisibility == "Hidden")
            {
                EveryTestControlsVisibility = "Visible";
            }
            else
            {
                EveryTestControlsVisibility = "Hidden";
            }
        }

        //Перемешать слова.
        public void MixWords()
        {
            if (!AscendingChecked)
            {
                ObservableCollection<Eng_Ukr> tempTestedWords = new ObservableCollection<Eng_Ukr>();
                Copy(testedWords, tempTestedWords);
                testedWords.Clear();

                if (DescendingChecked)
                {
                    testedWords = (MixWordsByDescending(tempTestedWords));
                }
                else if (SimpleRandomChecked)
                {
                    testedWords = (MixWordsBySimpleRandom(tempTestedWords));
                }
                else
                {
                    testedWords = (MixByCleverSelection(tempTestedWords));
                }
            }
        }

        //Перемешать слова в обратном порядке алфавита.
        public ObservableCollection<Eng_Ukr> MixWordsByDescending(ObservableCollection<Eng_Ukr> words)
        {
            ObservableCollection<Eng_Ukr> result = new ObservableCollection<Eng_Ukr>();

            for (int i = words.Count - 1; i >= 0; i--)
            {
                result.Add(words[i]);
            }

            return result;
        }

        //Перемешать слова простым рандомом.
        public ObservableCollection<Eng_Ukr> MixWordsBySimpleRandom(ObservableCollection<Eng_Ukr> words)
        {
            Random r = new Random();

            SortedList<int, Eng_Ukr> mixedList = new SortedList<int, Eng_Ukr>();

            foreach (var item in words)
            {
                mixedList.Add(r.Next(), item);
            }

            words.Clear();
            for (int i = 0; i < mixedList.Count; i++)
            {
                words.Add(mixedList.Values[i]);
            }

            return words;
        }

        //Перемешать слова "умным рандомом".
        public ObservableCollection<Eng_Ukr> MixByCleverSelection(ObservableCollection<Eng_Ukr> words)
        {
            ObservableCollection<Eng_Ukr> result = new ObservableCollection<Eng_Ukr>();

            ObservableCollection<Eng_Ukr> partOfResult = new ObservableCollection<Eng_Ukr>();

            var selectedWordsWithNullInTotal = (from item in words
                                                where item.Correct + item.Incorrect == 0
                                                select item);

            IEnumerable<Eng_Ukr> wordsWithNullInTotal = selectedWordsWithNullInTotal;

            foreach (var item in wordsWithNullInTotal)
            {
                partOfResult.Add(item);
            }

            partOfResult = MixWordsBySimpleRandom(partOfResult);

            foreach (var item in partOfResult)
            {
                result.Add(item);
            }

            partOfResult.Clear();

            int maximumOfIncorrect = (int)words.Max(Eng_Ukr => Eng_Ukr.Incorrect);

            for (int i = maximumOfIncorrect; i > 0; i--)
            {
                var selectedWordsWithCertainNumberOfIncorrect = (from item in words
                                                                 where item.Incorrect == i
                                                                 select item);

                IEnumerable<Eng_Ukr> wordsWithCertainNumberOfIncorrect = selectedWordsWithCertainNumberOfIncorrect;

                foreach (var item in wordsWithCertainNumberOfIncorrect)
                {
                    partOfResult.Add(item);
                }

                partOfResult = MixWordsBySimpleRandom(partOfResult);

                foreach (var item in partOfResult)
                {
                    result.Add(item);
                }

                partOfResult.Clear();
            }

            var selectedWordsWithNullInIncorrect = (from item in words
                                                    where item.Incorrect == 0 && item.Correct != 0
                                                    select item);

            IEnumerable<Eng_Ukr> wordsWithNullInIncorrect = selectedWordsWithNullInIncorrect;

            foreach (var item in wordsWithNullInIncorrect)
            {
                partOfResult.Add(item);
            }

            partOfResult = MixWordsBySimpleRandom(partOfResult);

            foreach (var item in partOfResult)
            {
                result.Add(item);
            }

            return result;
        }

        #endregion

        #endregion
    }
}