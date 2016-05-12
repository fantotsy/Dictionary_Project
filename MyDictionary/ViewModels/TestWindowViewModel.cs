using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MyDictionary.MVVMSupport;
using MyDictionary.ViewModels;
using System.Windows;

namespace MyDictionary.ViewModels
{
    class TestWindowViewModel : WindowViewModel
    {
        //#region Fields

        //private DictionaryDBEntities dataContext;

        //private int iterator = 0;

        //#endregion

        //#region Constructors

        //public TestWindowViewModel()
        //{
        //    dataContext = new DictionaryDBEntities();
        //    testedWords = new ObservableCollection<Eng_Ukr>(dataContext.Eng_Ukr);
        //    MixWords();

        //    Know = new DelegateCommand(RightAnswer);
        //    DoNotKnow = new DelegateCommand(WrongAnswer);
        //    Next = new DelegateCommand(NextWord);

        //    Word = testedWords[iterator].English;
        //}

        //#endregion

        ////Список тестируемых слов.
        ////public ObservableCollection<Eng_Ukr> testedWords { get; set; }

        ////Свойство, связанное с Label-вопросом.
        ////private string word;

        ////public string Word
        ////{
        ////    get { return word; }
        ////    set
        ////    {
        ////        word = value;
        ////        OnPropertyChanged("Word");
        ////    }
        ////}

        ////private string translation;

        //////Свойство, связанное с Label-ответом.
        ////public string Translation
        ////{
        ////    get { return translation; }
        ////    set
        ////    {
        ////        translation = value;
        ////        OnPropertyChanged("Translation");
        ////    }
        ////}

        ////#region Commands

        ////private void RightAnswer()
        ////{
        ////    testedWords[iterator].Correct++;
        ////    Translation = testedWords[iterator].Translation;
        ////    dataContext.SaveChanges();
        ////}

        ////public DelegateCommand Know { get; set; }

        ////private void WrongAnswer()
        ////{
        ////    testedWords[iterator].Incorrect++;
        ////    Translation = testedWords[iterator].Translation;
        ////    dataContext.SaveChanges();
        ////}

        ////public DelegateCommand DoNotKnow { get; set; }

        ////private void NextWord()
        ////{
        ////    Translation = "";
        ////    UpdateWord();
        ////}

        ////public DelegateCommand Next { get; set; }

        ////#endregion

        //#region Additional Components

        //////Реализация метода Dispose для освобождения ресурсов контекста при удалении ViewModel.
        ////public void Dispose()
        ////{
        ////    dataContext.Dispose();
        ////}

        ////public void UpdateWord()
        ////{
        ////    iterator++;
        ////    if (iterator < testedWords.Count)
        ////    {
        ////        Word = testedWords[iterator].English;
        ////    }
        ////    else
        ////    {
        ////        MessageBox.Show("Congratulations!");
        ////    }
        ////}

        //public void MixWords()
        //{
        //    if (RadioButtonDetails != 2)
        //    {
        //        ObservableCollection<Eng_Ukr> tempTestedWords = new ObservableCollection<Eng_Ukr>();
        //        Copy(testedWords, tempTestedWords);
        //        testedWords.Clear();

        //        if (RadioButtonDetails == 3)
        //        {
        //            testedWords = (MixWordsByDescending(tempTestedWords));
        //        }
        //        else if (RadioButtonDetails == 4)
        //        {
        //            testedWords = (MixWordsBySimpleRandom(tempTestedWords));
        //        }
        //        else if (RadioButtonDetails == 1)
        //        {
        //            testedWords = (MixByCleverSelection(tempTestedWords));
        //        }
        //    }
        //}

        

        //#region Методы перемешивания слов

        

        //#endregion

        //#endregion
    }
}
