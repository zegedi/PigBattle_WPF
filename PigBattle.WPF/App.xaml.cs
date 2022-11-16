using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using PigBattle.Model;
using PigBattle.Persistence;
using PigBattle.WPF.ViewModel;

namespace PigBattle.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Fields

        private PigBattleGameModel _model = null!;
        private PigBattleViewModel _viewModel = null!;
        private MainWindow _view = null!;

        #endregion

        #region Constructors

        /// <summary>
        /// Alkalmazás példányosítása.
        /// </summary>
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application event handlers

        private void App_Startup(object? sender, StartupEventArgs e)
        {
            // modell létrehozása
            _model = new PigBattleGameModel(new PigBattleFileDataAccess());
            _model.GameOver += new EventHandler<PigBattleEventArgs>(Model_GameOver);
            _model.NewGame();

            // nézemodell létrehozása
            _viewModel = new PigBattleViewModel(_model);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.LoadGame += new EventHandler(ViewModel_LoadGame);
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGame);
            _viewModel.InputError += new EventHandler(ViewModel_InputError);

            // nézet létrehozása
            _view = new MainWindow();
            _view.DataContext = _viewModel;
            _view.Closing += new System.ComponentModel.CancelEventHandler(View_Closing); // eseménykezelés a bezáráshoz
            _view.Show();
        }

        #endregion

        #region View event handlers

        /// <summary>
        /// Nézet bezárásának eseménykezelője.
        /// </summary>
        private void View_Closing(object? sender, CancelEventArgs e)
        {
            if (MessageBox.Show(
                "Biztosan ki szeretnél lépni?",
                "Harcos Robotmalacok csatája",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region ViewModel event handlers

        /// <summary>
        /// Új játék indításának eseménykezelője.
        /// </summary>
        private void ViewModel_NewGame(object? sender, EventArgs e)
        {
            _model.NewGame();
        }

        private async void ViewModel_LoadGame(object? sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Harcos Malacok csatája - Játéktábla betöltése";
            openFileDialog.Filter = "Marcos Malacok tábla|*.stl";
                
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _model.LoadGameAsync(openFileDialog.FileName);
                }
                catch (PigBattleDataException)
                {
                    MessageBox.Show(
                        "Játék betöltése sikertelen!" + Environment.NewLine +
                        "Hibás az elérési út, vagy a fájlformátum.",
                        "Hiba!", MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        /// <summary>
        /// Játék mentésének eseménykezelője.
        /// </summary>
        private async void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog(); // dialógablak
                saveFileDialog.Title = "Harcos Malacok csatája - Játéktábla mentése";
                saveFileDialog.Filter = "Marcos Malacok tábla|*.stl";
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveGameAsync(saveFileDialog.FileName);
                    }
                    catch (PigBattleDataException)
                    {
                        MessageBox.Show(
                            "Játék mentése sikertelen!" + Environment.NewLine +
                            "Hibás az elérési út, vagy a könyvtár nem írható.",
                            "Hiba!", MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
            catch
            {
                MessageBox.Show(
                    "Játék mentése sikertelen!", "Hiba!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Játékból való kilépés eseménykezelője.
        /// </summary>
        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            _view.Close();
        }

        /// <summary>
        /// A programInputButton megnyomásának eseménykezelője.
        /// </summary>
        private void ViewModel_InputError(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Hiba van a bemeneti programban." + Environment.NewLine + "Kérem adjon meg egy helyes programot!",
                "Figyelmeztetés! Helytelen bemenet.",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }


        #endregion


        #region Model event handlers

        /// <summary>
        /// Játék végének eseménykezelője.
        /// </summary>
        private void Model_GameOver(object? sender, PigBattleEventArgs e)
        {
            if (e.PlayerIndex == 3)
            {
                MessageBox.Show(
                    "A játék döntetlennel ért véget.",
                    "Játék vége!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Asterisk
                );
            }
            else
            {
                MessageBox.Show(
                    "A(z) " + e.PlayerIndex + " játékos nyert! Gratulálok!",
                    "Játék vége!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Asterisk
                );
            }

            _model.NewGame();
        }

        #endregion

    }
}
