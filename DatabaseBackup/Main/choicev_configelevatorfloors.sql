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
-- Table structure for table `configelevatorfloors`
--

DROP TABLE IF EXISTS `configelevatorfloors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configelevatorfloors` (
  `level` int NOT NULL AUTO_INCREMENT,
  `elevatorId` int NOT NULL,
  `name` varchar(50) NOT NULL,
  `door1Id` int NOT NULL,
  `door2Id` int NOT NULL,
  `callCollPos` text NOT NULL,
  `callCollHeight` float NOT NULL,
  `callCollWidth` float NOT NULL,
  `callCollRotation` float NOT NULL,
  `insideCollPos` text NOT NULL,
  `insideCollHeight` float NOT NULL,
  `insideCollWidth` float NOT NULL,
  `insideCollRotation` float NOT NULL,
  `codeElevator` varchar(45) NOT NULL,
  PRIMARY KEY (`level`,`elevatorId`)
) ENGINE=InnoDB AUTO_INCREMENT=107 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configelevatorfloors`
--

LOCK TABLES `configelevatorfloors` WRITE;
/*!40000 ALTER TABLE `configelevatorfloors` DISABLE KEYS */;
INSERT INTO `configelevatorfloors` VALUES (0,3,'',497,498,'{\"X\":51.11648,\"Y\":-1012.23737,\"Z\":28.330444}',1.8,1.2,69.6,'{\"X\":50.32308,\"Y\":-1014.41315,\"Z\":29.330444}',2.5,2.5,69.9,'PhysicalElevatorFloor'),(0,4,'',506,507,'{\"X\":-339.35715,\"Y\":-267.45386,\"Z\":36.053467}',3.5,2.3,53.8,'{\"X\":-341.15604,\"Y\":-270.1099,\"Z\":36.053467}',3.3,3.2,53.6,'PhysicalElevatorFloor'),(0,7,'',522,523,'{\"X\":331.64728,\"Y\":-808.8429,\"Z\":28.414673}',1,2.2,71,'{\"X\":333.44727,\"Y\":-809.74286,\"Z\":29.414673}',2.6,2.5,69.3,'PhysicalElevatorFloor'),(0,8,'',544,545,'{\"X\":-153.06044,\"Y\":6300.509,\"Z\":30.470337}',1.8,1,45,'{\"X\":-154.46045,\"Y\":6299.109,\"Z\":30.470337}',2.6,2.5,44.8,'PhysicalElevatorFloor'),(0,10,'EG: Lobby',547,546,'{\"X\":759.5285,\"Y\":227.4832,\"Z\":87.41174}',2.70078,1.01879,59.353,'{\"X\":760.48267,\"Y\":229.2425,\"Z\":87.41174}',2.54897,2.62223,60.041,'PhysicalElevatorFloor'),(1,1,'',491,492,'{\"X\":-2359.6372,\"Y\":3251.601,\"Z\":31.801514}',2.7,1.2,59.7,'{\"X\":-2360.7715,\"Y\":3249.5044,\"Z\":31.801514}',2.5,2.2,60,'PhysicalElevatorFloor'),(1,3,'',495,496,'{\"X\":52.32967,\"Y\":-1013.4846,\"Z\":47.337036}',1.8,1.2,72.1,'{\"X\":51.4011,\"Y\":-1015.67035,\"Z\":48.337036}',2.5,2.4,69.5,'DimensionElevatorFloor'),(1,4,'',504,505,'{\"X\":-339.33627,\"Y\":-267.52527,\"Z\":39.457153}',2.9,2,54.5,'{\"X\":-341.1736,\"Y\":-270.1022,\"Z\":39.457153}',3.4,3.3,53.9,'PhysicalElevatorFloor'),(1,5,'EG',514,515,'{\"X\":245.53847,\"Y\":-1099.1968,\"Z\":28.279907}',2.1,1.2,0,'{\"X\":247.81209,\"Y\":-1099.166,\"Z\":29.279907}',2.5,2.5,89.9,'PhysicalElevatorFloor'),(1,6,'',518,519,'{\"X\":135.38022,\"Y\":-764.0033,\"Z\":44.742065}',3,1.2,71,'{\"X\":136.10109,\"Y\":-761.7857,\"Z\":44.742065}',2.5,2.5,70,'PhysicalElevatorFloor'),(1,7,'',527,528,'{\"X\":335.66702,\"Y\":-807.31757,\"Z\":45.33191}',-0.9,1.8,68,'{\"X\":337.55493,\"Y\":-807.89777,\"Z\":46.33191}',2.5,2.5,70,'DimensionElevatorFloor'),(1,8,'',536,537,'{\"X\":-152.64507,\"Y\":6301.0737,\"Z\":35.221924}',1.6,1,45,'{\"X\":-154.04065,\"Y\":6299.62,\"Z\":35.221924}',2.5,2.6,45.6,'DimensionElevatorFloor'),(1,11,'1. Etage',561,562,'{\"X\":245.74721,\"Y\":-1090.8478,\"Z\":35.52527}',2.52001,0.876511,0,'{\"X\":247.76262,\"Y\":-1090.8156,\"Z\":36.52527}',2.58922,2.58547,0,'PhysicalElevatorFloor'),(1,12,'Emergency Control Centre',962,963,'{\"X\":316.45273,\"Y\":-311.9112,\"Z\":60.14868}',0.914953,2.4597,70.0871,'{\"X\":314.49573,\"Y\":-311.09418,\"Z\":60.14868}',2.57618,2.58959,69.8907,'PhysicalElevatorFloor'),(2,5,'1. Etage',512,513,'{\"X\":245.5121,\"Y\":-1099.0945,\"Z\":35.52527}',2.3,1.2,0,'{\"X\":247.7912,\"Y\":-1099.167,\"Z\":36.52527}',2.5,2.5,0,'PhysicalElevatorFloor'),(3,5,'Dachzugang',510,511,'{\"X\":248.65276,\"Y\":-1099.8989,\"Z\":41.321655}',3,1.4,0,'{\"X\":251.02089,\"Y\":-1099.9846,\"Z\":42.439575}',2.6,2.6,1.2,'PhysicalElevatorFloor'),(7,1,'',493,494,'{\"X\":-2359.6472,\"Y\":3251.6562,\"Z\":91.88794}',2.9,1.2,60.6,'{\"X\":-2360.757,\"Y\":3249.4934,\"Z\":91.88794}',2.5,2.2,59.8,'PhysicalElevatorFloor'),(7,10,'Etage 7: Daily Globe',563,564,'{\"X\":761.63696,\"Y\":232.44745,\"Z\":124.46448}',3.06889,1.21471,59.4802,'{\"X\":762.8745,\"Y\":234.09953,\"Z\":124.46448}',2.54661,2.63222,59.7142,'PhysicalElevatorFloor'),(8,10,'Etage 8: Daily Globe',565,566,'{\"X\":761.91736,\"Y\":232.41463,\"Z\":128.27258}',2.54897,0.885896,60.0255,'{\"X\":762.88293,\"Y\":234.09882,\"Z\":128.255615}',2.55142,2.62636,60.4956,'PhysicalElevatorFloor'),(8,14,'08:Vega § Blair',990,991,'{\"X\":-1457.1428,\"Y\":-551.67035,\"Z\":79.2395}',2.25594,1.15832,35.0418,'{\"X\":-1455.4963,\"Y\":-550.57275,\"Z\":79.2395}',2.58654,2.80475,35.1239,'PhysicalElevatorFloor'),(35,6,'',520,521,'{\"X\":134.12967,\"Y\":-758.0824,\"Z\":185.10107}',2.5,1.4,70.3,'{\"X\":134.78682,\"Y\":-755.9626,\"Z\":185.10107}',2.5,2.6,69.8,'PhysicalElevatorFloor'),(103,11,'EG',559,560,'{\"X\":245.7413,\"Y\":-1090.8065,\"Z\":28.279907}',2.35322,0.880014,0.119854,'{\"X\":247.76527,\"Y\":-1090.8113,\"Z\":29.279907}',2.59112,2.56913,0,'PhysicalElevatorFloor'),(104,12,'Erdgeschoss',960,961,'{\"X\":316.53098,\"Y\":-311.76584,\"Z\":52.38098}',0.939124,2.58041,70.1177,'{\"X\":314.4977,\"Y\":-311.10904,\"Z\":52.38098}',2.60409,2.61065,69.8189,'PhysicalElevatorFloor'),(106,14,'EG:Lobby',988,989,'{\"X\":-1448.8748,\"Y\":-542.7033,\"Z\":34.77295}',2.05405,1.44542,34.3768,'{\"X\":-1450.7034,\"Y\":-543.93134,\"Z\":34.77295}',2.56549,2.58953,35.0204,'PhysicalElevatorFloor');
/*!40000 ALTER TABLE `configelevatorfloors` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:11:51
