-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: game.choicev.net    Database: choicev
-- ------------------------------------------------------
-- Server version	8.0.25

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `configweapondamage`
--

DROP TABLE IF EXISTS `configweapondamage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configweapondamage` (
  `weaponint` varchar(45) NOT NULL,
  `damage` float NOT NULL,
  `weapon` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`weaponint`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configweapondamage`
--

LOCK TABLES `configweapondamage` WRITE;
/*!40000 ALTER TABLE `configweapondamage` DISABLE KEYS */;
INSERT INTO `configweapondamage` VALUES ('100416529',101,'SniperRifle'),('1119849093',30,'Minigun'),('1198256469',45,'UnholyHellbringer'),('1198879012',10,'FlareGun'),('1233104067',0,'Flare'),('126349499',0,'Snowballs'),('137902532',34,'VintagePistol'),('1432025498',32,'PumpShotgunMkII'),('1593441988',27,'CombatPistol'),('1627465347',34,'GusenbergSweeper'),('1649403952',34,'CompactRifle'),('1672152130',0,'HomingLauncher'),('171789620',28,'CombatPDW'),('1737195953',0,'Nightstick'),('177293209',230,'HeavySniperMkII'),('1785463520',75,'MarksmanRifleMkII'),('1834241177',30,'Railgun'),('2017895192',40,'SawedOffShotgun'),('2024373456',25,'SMGMkII'),('205991906',216,'HeavySniper'),('2132975508',32,'BullpupRifle'),('2138347493',0,'FireworkLauncher'),('2144741730',45,'CombatMG'),('2210333304',32,'CarbineRifle'),('2228681469',33,'BullpupRifleMkII'),('2481070269',0,'Grenade'),('2526821735',32.5,'SpecialCarbineMkII'),('2578377531',51,'Pistol50'),('2578778090',0,'Knife'),('2634544996',40,'MG'),('2640438543',14,'BullpupShotgun'),('2694266206',0,'BZGas'),('2726580491',0,'GrenadeLauncher'),('2828843422',165,'Musket'),('2874559379',0,'ProximityMines'),('2937143193',34,'AdvancedRifle'),('2982836145',0,'RPG'),('3125143736',0,'PipeBombs'),('317205821',27,'SweeperShotgun'),('3173288789',22,'MiniSMG'),('3218215474',28,'SNSPistol'),('3219281620',32,'PistolMkII'),('3220176749',30,'AssaultRifle'),('3231910285',32,'SpecialCarbine'),('324215364',21,'MicroSMG'),('3249783761',160,'HeavyRevolver'),('3342088282',65,'MarksmanRifle'),('3415619887',200,'HeavyRevolverMkII'),('3523564046',40,'HeavyPistol'),('3675956304',27,'MachinePistol'),('3686625920',47,'CombatMGMkII'),('3696079510',220,'MarksmanPistol'),('3800352039',32,'AssaultShotgun'),('4019527611',30,'DoubleBarrelShotgun'),('4024951519',23,'AssaultSMG'),('4208062921',33,'CarbineRifleMkII'),('4256991824',0,'TearGas'),('453432689',26,'Pistol'),('487013001',29,'PumpShotgun'),('584646201',25,'APPistol'),('615608432',0,'MolotovCocktail'),('727643628',32,NULL),('736523883',22,'SMG'),('741814745',0,'StickyBomb'),('984333226',117,'HeavyShotgun');
/*!40000 ALTER TABLE `configweapondamage` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:08:30
