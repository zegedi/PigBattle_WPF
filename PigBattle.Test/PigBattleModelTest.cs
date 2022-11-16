using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PigBattle.Model;
using PigBattle.Persistence;

namespace PigBattle.Test
{
    [TestClass]
    public class PigBattleModelTest
    {
        private PigBattleGameModel _model = null!;
        private PigBattleTable _mockTable = null!;
        private Mock<IPigBattleDataAccess> _mock = null!;

        [TestInitialize]
        public void Initialize()
        {
            // elõre definiálunk egy játéktáblát a perzisztencia mockolt teszteléséhez
            FieldType[,] fields = new FieldType[4, 4]
            {
                {FieldType.Empty,  FieldType.Empty, FieldType.Empty,  FieldType.Empty},
                {FieldType.Empty,  FieldType.Empty, FieldType.Player, FieldType.Empty},
                {FieldType.Player, FieldType.Empty, FieldType.Empty,  FieldType.Empty},
                {FieldType.Empty,  FieldType.Empty, FieldType.Empty,  FieldType.Empty}
            };
            RobotPig[] players = new RobotPig[2]
            {
                new RobotPig(2, 0, Direction.Up, 2), new RobotPig(1, 2, Direction.Left, 1)
            };
            _mockTable = new PigBattleTable(players, fields);

            // a mock a Load mûveletben bármilyen paraméterre az elõre beállított játéktáblát fogja visszaadni
            _mock = new Mock<IPigBattleDataAccess>();
            _mock.Setup(mock => mock.Load(It.IsAny<String>())).Returns(_mockTable);

            // példányosítjuk a modellt a mock objektummal
            _model = new PigBattleGameModel(_mock.Object);

            _model.GameAdvanced += new EventHandler<PigBattleEventArgs>(Model_GameAdvanced);
            _model.GameOver += new EventHandler<PigBattleEventArgs>(Model_GameOver);
        }

        [TestMethod]
        public void PigBattleModelLoadGameTest()
        {
            _model.LoadGame(String.Empty);

            Assert.AreEqual(TableSize.Small, _model.TableSize); //Megfelelõ táblaméret került-e beállításra
            Assert.AreEqual(_mockTable.TableContent.GetLength(0), _model.Table.TableContent.GetLength(0)); //ugyanannyi sor
            Assert.AreEqual(_mockTable.TableContent.GetLength(1), _model.Table.TableContent.GetLength(1)); //ugyanannyi oszlop

            //TableContentek ellenõrzése
            for (Int32 i = 0; i < 4; ++i)
                for (Int32 j = 0; j < 4; ++j)
                {
                    Assert.AreEqual(_mockTable.TableContent[i, j], _model.Table.TableContent[i, j]);
                }

            //Ugyanúgy két játékos van
            Assert.AreEqual(_mockTable.Players.Length, _model.Table.Players.Length);

            //PlayerOne összehasonlítás
            Assert.AreEqual(_mockTable.PlayerOne.X, _model.Table.PlayerOne.X);
            Assert.AreEqual(_mockTable.PlayerOne.Y, _model.Table.PlayerOne.Y);
            Assert.AreEqual(_mockTable.PlayerOne.Health, _model.Table.PlayerOne.Health);
            Assert.AreEqual(_mockTable.PlayerOne.Direction, _model.Table.PlayerOne.Direction);

            //PlayerTwo összehasonlítás
            Assert.AreEqual(_mockTable.PlayerTwo.X, _model.Table.PlayerTwo.X);
            Assert.AreEqual(_mockTable.PlayerTwo.Y, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(_mockTable.PlayerTwo.Health, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(_mockTable.PlayerTwo.Direction, _model.Table.PlayerTwo.Direction);
        }

        [TestMethod]
        public void PigBattleModelPlayRoundSmallSizeTest()
        {
            //Modell beállítása és új játék indítása
            _model.TableSize = TableSize.Small;
            _model.NewGame();

            /****************** Kezdetben ******************/

            //Kezdetben egy a RoundCount
            Assert.AreEqual(1, _model.RoundCount);

            //PlayerOne kezdeti beálltásai
            Assert.AreEqual(2, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo kezdeti beálltásai
            Assert.AreEqual(1, _model.Table.PlayerTwo.X);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "balra, tûz, tûz, tûz, fordulás jobbra",
                "fordulás balra, fordulás balra, fordulás balra, fordulás jobbra, tûz"
            );

            /******************* 1. Kör után *******************/

            //RoundCount
            Assert.AreEqual(2, _model.RoundCount);

            //PlayerOne állapota
            Assert.AreEqual(1, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo állapota
            Assert.AreEqual(1, _model.Table.PlayerTwo.X);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerTwo.Direction);
        }

        [TestMethod]
        public void PigBattleModelPlayRoundMediumSizeTest()
        {
            //Modell beállítása és új játék indítása
            _model.TableSize = TableSize.Medium;
            _model.NewGame();

            /****************** Kezdetben ******************/

            //Kezdetben nulla a RoundCount
            Assert.AreEqual(1, _model.RoundCount);

            //PlayerOne kezdeti beálltásai
            Assert.AreEqual(3, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo kezdeti beálltásai
            Assert.AreEqual(2, _model.Table.PlayerTwo.X);
            Assert.AreEqual(5, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);


            _model.PlayRound(
                "balra, tûz, elõre, elõre, ütés",
                "elõre, elõre, balra, tûz, tûz"
            );

            /******************* 1. Kör után *******************/

            //RoundCount
            Assert.AreEqual(2, _model.RoundCount);

            //PlayerOne állapota
            Assert.AreEqual(2, _model.Table.PlayerOne.X);
            Assert.AreEqual(2, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo állapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(1, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "hátra, hátra, jobbra, tûz, tûz",
                "ütés, elõre, fordulás jobbra, tûz, tûz"
            );

            /******************* 2. Kör után *******************/

            //RoundCount
            Assert.AreEqual(3, _model.RoundCount);

            //PlayerOne állapota
            Assert.AreEqual(3, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo állapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(2, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Up, _model.Table.PlayerTwo.Direction);
        }

        [TestMethod]
        public void PigBattleModelPlayRoundLargeSizeTest()
        {
            //Modell beállítása és új játék indítása
            _model.TableSize = TableSize.Large;
            _model.NewGame();

            /****************** Kezdetben ******************/

            //Kezdetben egy a RoundCount
            Assert.AreEqual(1, _model.RoundCount);

            //PlayerOne kezdeti beálltásai
            Assert.AreEqual(4, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo kezdeti beálltásai
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(7, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "elõre, elõre, elõre, elõre, elõre",
                "tûz, elõre, elõre, fordulás balra, elõre"
            );

            /******************* 1. Kör után *******************/

            //RoundCount
            Assert.AreEqual(2, _model.RoundCount);

            //PlayerOne állapota
            Assert.AreEqual(4, _model.Table.PlayerOne.X);
            Assert.AreEqual(4, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo állapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(5, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Down, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "ütés, ütés, ütés, ütés, ütés",
                "ütés, ütés, ütés, ütés, ütés"
            );

            /******************* 2. Kör után *******************/

            //RoundCount
            Assert.AreEqual(3, _model.RoundCount);

            //PlayerOne állapota
            Assert.AreEqual(4, _model.Table.PlayerOne.X);
            Assert.AreEqual(4, _model.Table.PlayerOne.Y);
            Assert.AreEqual(0, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo állapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(5, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Down, _model.Table.PlayerTwo.Direction);
        }


        [TestMethod]
        public void PigBattleModelIsValidProgramTest()
        {
            //Helyes bemeneti programok
            Assert.IsTrue(_model.IsValidProgram("elõre,hátra,jobbra,balra,tûz"));
            Assert.IsTrue(_model.IsValidProgram("fordulás balra, fordulás jobbra, ütés, hátra, hátra"));
            Assert.IsTrue(_model.IsValidProgram("tûz,tûz,tûz,tûz,tûz"));
            Assert.IsTrue(_model.IsValidProgram("fordulás balra, fordulás jobbra, fordulás balra, fordulás jobbra, fordulás balra"));
            Assert.IsTrue(_model.IsValidProgram(" HÁTRA ,  JoBbrA  , ÜtÉs,   FOrDulÁs BalRA,   TÛz    "));

            //Helytelen bemeneti programok
            Assert.IsFalse(_model.IsValidProgram(""));
            Assert.IsFalse(_model.IsValidProgram("tûz,elõre,hátra"));
            Assert.IsFalse(_model.IsValidProgram("fordulás jobbra,fordulás balra,hátra,jobbra"));
            Assert.IsFalse(_model.IsValidProgram("jobbra,ütés,jobbra,fordulás balra,tûz,balra"));
            Assert.IsFalse(_model.IsValidProgram("elõre,hátra,jobbra,balra,tûz,tûz,tûz,tûz"));
            Assert.IsFalse(_model.IsValidProgram("elõre,ugrás,vissza,támadás,bummm"));
        }

        private void Model_GameAdvanced(Object? sender, PigBattleEventArgs e)
        {
            Assert.AreEqual(_model.RoundCount, e.RoundCount); //Megegyezik-e a körszám
            Assert.AreEqual(0, e.PlayerIndex); //Nincs gyõztese a játéknak még
        }

        private void Model_GameOver(Object? sender, PigBattleEventArgs e)
        {
            Assert.IsTrue(e.RoundOver); //Végevan-e a körnek is
            Assert.AreEqual(_model.RoundCount, e.RoundCount); //Megegyezik-e a körszám
            Assert.AreNotEqual(0, _model.Table.WinnerPlayerNumber()); //Tényleg vége van a játéknak

            //Ha döntetlen akkor tényleg mindkét játékos halott
            if (_model.Table.WinnerPlayerNumber() == 3)
            {
                Assert.AreEqual(0, _model.Table.PlayerOne.Health);
                Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            }
            else
            {
                Assert.AreEqual(e.PlayerIndex, _model.Table.WinnerPlayerNumber()); //Valóban õ a gyõztes
            }
        }
    }
}