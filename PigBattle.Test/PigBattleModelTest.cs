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
            // el�re defini�lunk egy j�t�kt�bl�t a perzisztencia mockolt tesztel�s�hez
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

            // a mock a Load m�veletben b�rmilyen param�terre az el�re be�ll�tott j�t�kt�bl�t fogja visszaadni
            _mock = new Mock<IPigBattleDataAccess>();
            _mock.Setup(mock => mock.Load(It.IsAny<String>())).Returns(_mockTable);

            // p�ld�nyos�tjuk a modellt a mock objektummal
            _model = new PigBattleGameModel(_mock.Object);

            _model.GameAdvanced += new EventHandler<PigBattleEventArgs>(Model_GameAdvanced);
            _model.GameOver += new EventHandler<PigBattleEventArgs>(Model_GameOver);
        }

        [TestMethod]
        public void PigBattleModelLoadGameTest()
        {
            _model.LoadGame(String.Empty);

            Assert.AreEqual(TableSize.Small, _model.TableSize); //Megfelel� t�blam�ret ker�lt-e be�ll�t�sra
            Assert.AreEqual(_mockTable.TableContent.GetLength(0), _model.Table.TableContent.GetLength(0)); //ugyanannyi sor
            Assert.AreEqual(_mockTable.TableContent.GetLength(1), _model.Table.TableContent.GetLength(1)); //ugyanannyi oszlop

            //TableContentek ellen�rz�se
            for (Int32 i = 0; i < 4; ++i)
                for (Int32 j = 0; j < 4; ++j)
                {
                    Assert.AreEqual(_mockTable.TableContent[i, j], _model.Table.TableContent[i, j]);
                }

            //Ugyan�gy k�t j�t�kos van
            Assert.AreEqual(_mockTable.Players.Length, _model.Table.Players.Length);

            //PlayerOne �sszehasonl�t�s
            Assert.AreEqual(_mockTable.PlayerOne.X, _model.Table.PlayerOne.X);
            Assert.AreEqual(_mockTable.PlayerOne.Y, _model.Table.PlayerOne.Y);
            Assert.AreEqual(_mockTable.PlayerOne.Health, _model.Table.PlayerOne.Health);
            Assert.AreEqual(_mockTable.PlayerOne.Direction, _model.Table.PlayerOne.Direction);

            //PlayerTwo �sszehasonl�t�s
            Assert.AreEqual(_mockTable.PlayerTwo.X, _model.Table.PlayerTwo.X);
            Assert.AreEqual(_mockTable.PlayerTwo.Y, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(_mockTable.PlayerTwo.Health, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(_mockTable.PlayerTwo.Direction, _model.Table.PlayerTwo.Direction);
        }

        [TestMethod]
        public void PigBattleModelPlayRoundSmallSizeTest()
        {
            //Modell be�ll�t�sa �s �j j�t�k ind�t�sa
            _model.TableSize = TableSize.Small;
            _model.NewGame();

            /****************** Kezdetben ******************/

            //Kezdetben egy a RoundCount
            Assert.AreEqual(1, _model.RoundCount);

            //PlayerOne kezdeti be�llt�sai
            Assert.AreEqual(2, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo kezdeti be�llt�sai
            Assert.AreEqual(1, _model.Table.PlayerTwo.X);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "balra, t�z, t�z, t�z, fordul�s jobbra",
                "fordul�s balra, fordul�s balra, fordul�s balra, fordul�s jobbra, t�z"
            );

            /******************* 1. K�r ut�n *******************/

            //RoundCount
            Assert.AreEqual(2, _model.RoundCount);

            //PlayerOne �llapota
            Assert.AreEqual(1, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo �llapota
            Assert.AreEqual(1, _model.Table.PlayerTwo.X);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerTwo.Direction);
        }

        [TestMethod]
        public void PigBattleModelPlayRoundMediumSizeTest()
        {
            //Modell be�ll�t�sa �s �j j�t�k ind�t�sa
            _model.TableSize = TableSize.Medium;
            _model.NewGame();

            /****************** Kezdetben ******************/

            //Kezdetben nulla a RoundCount
            Assert.AreEqual(1, _model.RoundCount);

            //PlayerOne kezdeti be�llt�sai
            Assert.AreEqual(3, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo kezdeti be�llt�sai
            Assert.AreEqual(2, _model.Table.PlayerTwo.X);
            Assert.AreEqual(5, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);


            _model.PlayRound(
                "balra, t�z, el�re, el�re, �t�s",
                "el�re, el�re, balra, t�z, t�z"
            );

            /******************* 1. K�r ut�n *******************/

            //RoundCount
            Assert.AreEqual(2, _model.RoundCount);

            //PlayerOne �llapota
            Assert.AreEqual(2, _model.Table.PlayerOne.X);
            Assert.AreEqual(2, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo �llapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(1, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "h�tra, h�tra, jobbra, t�z, t�z",
                "�t�s, el�re, fordul�s jobbra, t�z, t�z"
            );

            /******************* 2. K�r ut�n *******************/

            //RoundCount
            Assert.AreEqual(3, _model.RoundCount);

            //PlayerOne �llapota
            Assert.AreEqual(3, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo �llapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(2, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Up, _model.Table.PlayerTwo.Direction);
        }

        [TestMethod]
        public void PigBattleModelPlayRoundLargeSizeTest()
        {
            //Modell be�ll�t�sa �s �j j�t�k ind�t�sa
            _model.TableSize = TableSize.Large;
            _model.NewGame();

            /****************** Kezdetben ******************/

            //Kezdetben egy a RoundCount
            Assert.AreEqual(1, _model.RoundCount);

            //PlayerOne kezdeti be�llt�sai
            Assert.AreEqual(4, _model.Table.PlayerOne.X);
            Assert.AreEqual(0, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo kezdeti be�llt�sai
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(7, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Left, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "el�re, el�re, el�re, el�re, el�re",
                "t�z, el�re, el�re, fordul�s balra, el�re"
            );

            /******************* 1. K�r ut�n *******************/

            //RoundCount
            Assert.AreEqual(2, _model.RoundCount);

            //PlayerOne �llapota
            Assert.AreEqual(4, _model.Table.PlayerOne.X);
            Assert.AreEqual(4, _model.Table.PlayerOne.Y);
            Assert.AreEqual(3, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo �llapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(5, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(3, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Down, _model.Table.PlayerTwo.Direction);

            _model.PlayRound(
                "�t�s, �t�s, �t�s, �t�s, �t�s",
                "�t�s, �t�s, �t�s, �t�s, �t�s"
            );

            /******************* 2. K�r ut�n *******************/

            //RoundCount
            Assert.AreEqual(3, _model.RoundCount);

            //PlayerOne �llapota
            Assert.AreEqual(4, _model.Table.PlayerOne.X);
            Assert.AreEqual(4, _model.Table.PlayerOne.Y);
            Assert.AreEqual(0, _model.Table.PlayerOne.Health);
            Assert.AreEqual(Direction.Right, _model.Table.PlayerOne.Direction);

            //PlayerTwo �llapota
            Assert.AreEqual(3, _model.Table.PlayerTwo.X);
            Assert.AreEqual(5, _model.Table.PlayerTwo.Y);
            Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            Assert.AreEqual(Direction.Down, _model.Table.PlayerTwo.Direction);
        }


        [TestMethod]
        public void PigBattleModelIsValidProgramTest()
        {
            //Helyes bemeneti programok
            Assert.IsTrue(_model.IsValidProgram("el�re,h�tra,jobbra,balra,t�z"));
            Assert.IsTrue(_model.IsValidProgram("fordul�s balra, fordul�s jobbra, �t�s, h�tra, h�tra"));
            Assert.IsTrue(_model.IsValidProgram("t�z,t�z,t�z,t�z,t�z"));
            Assert.IsTrue(_model.IsValidProgram("fordul�s balra, fordul�s jobbra, fordul�s balra, fordul�s jobbra, fordul�s balra"));
            Assert.IsTrue(_model.IsValidProgram(" H�TRA ,  JoBbrA  , �t�s,   FOrDul�s BalRA,   T�z    "));

            //Helytelen bemeneti programok
            Assert.IsFalse(_model.IsValidProgram(""));
            Assert.IsFalse(_model.IsValidProgram("t�z,el�re,h�tra"));
            Assert.IsFalse(_model.IsValidProgram("fordul�s jobbra,fordul�s balra,h�tra,jobbra"));
            Assert.IsFalse(_model.IsValidProgram("jobbra,�t�s,jobbra,fordul�s balra,t�z,balra"));
            Assert.IsFalse(_model.IsValidProgram("el�re,h�tra,jobbra,balra,t�z,t�z,t�z,t�z"));
            Assert.IsFalse(_model.IsValidProgram("el�re,ugr�s,vissza,t�mad�s,bummm"));
        }

        private void Model_GameAdvanced(Object? sender, PigBattleEventArgs e)
        {
            Assert.AreEqual(_model.RoundCount, e.RoundCount); //Megegyezik-e a k�rsz�m
            Assert.AreEqual(0, e.PlayerIndex); //Nincs gy�ztese a j�t�knak m�g
        }

        private void Model_GameOver(Object? sender, PigBattleEventArgs e)
        {
            Assert.IsTrue(e.RoundOver); //V�gevan-e a k�rnek is
            Assert.AreEqual(_model.RoundCount, e.RoundCount); //Megegyezik-e a k�rsz�m
            Assert.AreNotEqual(0, _model.Table.WinnerPlayerNumber()); //T�nyleg v�ge van a j�t�knak

            //Ha d�ntetlen akkor t�nyleg mindk�t j�t�kos halott
            if (_model.Table.WinnerPlayerNumber() == 3)
            {
                Assert.AreEqual(0, _model.Table.PlayerOne.Health);
                Assert.AreEqual(0, _model.Table.PlayerTwo.Health);
            }
            else
            {
                Assert.AreEqual(e.PlayerIndex, _model.Table.WinnerPlayerNumber()); //Val�ban � a gy�ztes
            }
        }
    }
}