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
-- Table structure for table `configvehiclesclasses`
--

DROP TABLE IF EXISTS `configvehiclesclasses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configvehiclesclasses` (
  `classId` int NOT NULL,
  `createDate` datetime NOT NULL,
  `ClassName` varchar(50) NOT NULL,
  `InventorySize` int NOT NULL DEFAULT '0',
  `FuelMax` float NOT NULL DEFAULT '0',
  `FuelPerKm` float NOT NULL DEFAULT '0',
  `FuelPerMin` float NOT NULL DEFAULT '0',
  `FuelType` int NOT NULL DEFAULT '0',
  `EnginePerKm` float NOT NULL DEFAULT '0',
  `WheelPerKm` float NOT NULL DEFAULT '0',
  PRIMARY KEY (`classId`),
  KEY `Class` (`classId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configvehiclesclasses`
--

LOCK TABLES `configvehiclesclasses` WRITE;
/*!40000 ALTER TABLE `configvehiclesclasses` DISABLE KEYS */;
INSERT INTO `configvehiclesclasses` VALUES (0,'2020-03-30 00:00:00','Compacts',50,40,0,0,1,0.05,0.066),(1,'2020-03-30 00:00:00','Sedans',100,60,0,0,1,0.05,0.066),(2,'2020-03-30 00:00:00','SUVs',150,60,0,0,1,0.05,0.066),(3,'2020-03-30 00:00:00','Coupes',50,60,0,0,1,0.05,0.066),(4,'2020-03-30 00:00:00','Muscle',100,60,0,0,1,0.05,0.066),(5,'2020-03-30 00:00:00','Sports Classics',100,60,0,0,1,0.05,0.066),(6,'2020-03-30 00:00:00','Sports',100,60,0,0,1,0.05,0.066),(7,'2020-03-30 00:00:00','Super',50,60,0,0,1,0.05,0.066),(8,'2020-03-30 00:00:00','Motorcycles',25,30,0,0,1,0.05,0.066),(9,'2020-03-30 00:00:00','Off-road',100,60,0,0,1,0.05,0.066),(10,'2020-03-30 00:00:00','Industrial',100,120,0,0,1,0.05,0.066),(11,'2020-03-30 00:00:00','Utility',100,120,0,0,2,0.05,0.066),(12,'2020-03-30 00:00:00','Vans',100,80,0,0,1,0.05,0.066),(13,'2020-03-30 00:00:00','Cycles',25,0,0,0,0,0,0),(14,'2020-03-30 00:00:00','Boats',100,30,0,0,1,0.05,0.066),(15,'2020-03-30 00:00:00','Helicopters',200,600,0,0,3,0.05,0.066),(16,'2020-03-30 00:00:00','Planes',100,600,0,0,3,0.05,0.066),(17,'2020-03-30 00:00:00','Service',100,120,0,0,1,0.05,0.066),(18,'2020-03-30 00:00:00','Emergency',120,80,0,0,1,0.05,0.066),(19,'2020-03-30 00:00:00','Military',100,80,0,0,1,0.05,0.066),(20,'2020-03-30 00:00:00','Commercial',400,120,0,0,2,0.05,0.066),(21,'2020-03-30 00:00:00','Trains',100,0,0,0,0,0,0),(22,'2020-03-30 00:00:00','Open Wheels',0,120,0,0,1,0.05,0.066);
/*!40000 ALTER TABLE `configvehiclesclasses` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:11:21
