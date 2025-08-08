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
-- Table structure for table `confighotelroomfeatures`
--

DROP TABLE IF EXISTS `confighotelroomfeatures`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighotelroomfeatures` (
  `id` int NOT NULL AUTO_INCREMENT,
  `hotelRoomId` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `colPos` text NOT NULL,
  `colWidth` float NOT NULL,
  `colHeight` float NOT NULL,
  `colRotation` float NOT NULL,
  `codeInstance` varchar(45) NOT NULL,
  `settings` text NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_confighotelrooms_confighotelroomfeatures_hotelRoomId_idx` (`hotelRoomId`),
  CONSTRAINT `fk_confighotelrooms_confighotelroomfeatures_hotelRoomId` FOREIGN KEY (`hotelRoomId`) REFERENCES `confighotelrooms` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighotelroomfeatures`
--

LOCK TABLES `confighotelroomfeatures` WRITE;
/*!40000 ALTER TABLE `confighotelroomfeatures` DISABLE KEYS */;
INSERT INTO `confighotelroomfeatures` VALUES (10,4,'Regal','{\"X\":52.625275,\"Y\":-1004.15607,\"Z\":47.370728}',2.6,1.4,66,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"25\"}'),(11,4,'sauberes Bad','{\"X\":56.918682,\"Y\":-1010.0154,\"Z\":47.370728}',2.3,1.4,67.3,'HotelRoomBathFeatureInstance',''),(12,2,'Regal','{\"X\":60.774723,\"Y\":-1007.1989,\"Z\":47.35388}',-2.8,1.6,67,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"25\"}'),(14,2,'sauberes Bad','{\"X\":65.31648,\"Y\":-1013.04504,\"Z\":47.370728}',2.4,1.5,68.7,'HotelRoomBathFeatureInstance',''),(15,5,'Regal','{\"X\":69.079124,\"Y\":-1010.3638,\"Z\":47.35388}',2.9,1.6,68.5,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"25\"}'),(17,5,'sauberes Bad','{\"X\":73.5967,\"Y\":-1016.2912,\"Z\":47.370728}',2.3,1.6,68,'HotelRoomBathFeatureInstance',''),(18,6,'Regal','{\"X\":72.32528,\"Y\":-1023.58026,\"Z\":47.370728}',1.4,2.6,66.7,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"25\"}'),(20,6,'sauberes Bad','{\"X\":66.42198,\"Y\":-1027.777,\"Z\":47.370728}',1.3,2.4,66.8,'HotelRoomBathFeatureInstance',''),(21,8,'Regal','{\"X\":54.91319,\"Y\":-1023.02637,\"Z\":47.35388}',1.8,2.5,68.8,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"25\"}'),(23,8,'sauberes Bad','{\"X\":60.65934,\"Y\":-1018.89233,\"Z\":47.370728}',1.4,2.3,67.2,'HotelRoomBathFeatureInstance',''),(24,9,'Regal','{\"X\":69.108795,\"Y\":-1031.777,\"Z\":47.370728}',1.7,2.9,68.7,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"25\"}'),(26,9,'sauberes Bad','{\"X\":63.40989,\"Y\":-1035.9054,\"Z\":47.370728}',-1.9,2.4,67.2,'HotelRoomBathFeatureInstance',''),(28,7,'Schrank','{\"X\":337.52417,\"Y\":-811.5726,\"Z\":45.33191}',1.1,2.5,69.6,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"17.5\"}'),(29,10,'Schrank','{\"X\":330.4747,\"Y\":-808.9934,\"Z\":45.348755}',-2.4,-1.5,158.7,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"17.5\"}'),(31,11,'Schrank','{\"X\":339.69232,\"Y\":-805.26263,\"Z\":45.3656}',1.3,2.6,70.8,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"17.5\"}'),(33,12,'Schrank','{\"X\":332.5769,\"Y\":-802.9769,\"Z\":45.3656}',2.8,1.4,159.9,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"17.5\"}'),(35,13,'Schrank','{\"X\":341.94064,\"Y\":-799.5242,\"Z\":45.3656}',2.6,1.4,161.1,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"17.5\"}'),(37,14,'Schrank','{\"X\":334.8088,\"Y\":-797.1,\"Z\":45.33191}',2.6,1.6,158.1,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"17.5\"}'),(42,16,'Kabinett','{\"X\":-142.3934,\"Y\":6301.2573,\"Z\":35.255737}',1.4,3,134.7,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"20\"}'),(43,15,'Kabinett','{\"X\":-151.99121,\"Y\":6292.1274,\"Z\":35.255737}',3.2,1.1,44,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"20\"}'),(44,19,'Kabinett','{\"X\":-147.43187,\"Y\":6287.557,\"Z\":35.255737}',3.1,1.1,44.2,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"20\"}'),(47,20,'Kabinett','{\"X\":-142.68132,\"Y\":6282.825,\"Z\":35.255737}',3.2,1.3,44.1,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"20\"}'),(48,17,'Kabinett','{\"X\":-137.95274,\"Y\":6296.7295,\"Z\":35.255737}',3,1.7,42,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"20\"}'),(51,18,'Kabinett','{\"X\":-133.52419,\"Y\":6292.175,\"Z\":35.255737}',3.1,1.3,44.7,'HotelRoomStorageFeatureInstance','{\"InventorySize\":\"20\"}');
/*!40000 ALTER TABLE `confighotelroomfeatures` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:25
