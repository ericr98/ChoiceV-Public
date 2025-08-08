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
-- Table structure for table `configvehicleuniversalmods`
--

DROP TABLE IF EXISTS `configvehicleuniversalmods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configvehicleuniversalmods` (
  `id` int NOT NULL AUTO_INCREMENT,
  `configvehiclemodtypes_id` int DEFAULT NULL,
  `ModIndex` int NOT NULL,
  `DisplayName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`id`),
  KEY `configvehicleuniversalmods_configvehiclemodtypes_id_fk` (`configvehiclemodtypes_id`),
  CONSTRAINT `configvehicleuniversalmods_configvehiclemodtypes_id_fk` FOREIGN KEY (`configvehiclemodtypes_id`) REFERENCES `configvehiclemodtypes` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=102 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configvehicleuniversalmods`
--

LOCK TABLES `configvehicleuniversalmods` WRITE;
/*!40000 ALTER TABLE `configvehicleuniversalmods` DISABLE KEYS */;
INSERT INTO `configvehicleuniversalmods` VALUES (1,13,-1,'Serie'),(2,13,0,'Straßenbremse'),(3,13,1,'Sportbremse'),(4,13,2,'Rennbremse'),(5,12,-1,'Serie'),(6,12,0,'EMS-Verbesserung 1'),(7,12,1,'EMS-Verbesserung 2'),(8,12,2,'EMS-Verbesserung 3'),(9,12,3,'EMS-Verbesserung 4'),(10,14,-1,'Serie'),(11,14,0,'Straßengetriebe'),(12,14,1,'Sportgetriebe'),(13,14,2,'Renngetriebe'),(14,14,3,'Supergetriebe'),(15,16,-1,'Serie'),(16,16,0,'Tiefergelegte Federung'),(17,16,1,'Straßenfederung'),(18,16,2,'Sportfederung'),(19,16,3,'Wettkampffederung'),(20,18,-1,'Ohne Turbo'),(21,18,0,'Mit Turbo'),(22,19,-1,'Halogen'),(23,19,0,'Xenon'),(24,35,-1,'Serie'),(25,35,0,'Helles Rauchglas'),(26,35,1,'Dunkles Rauchglas'),(27,35,2,'Limousine'),(28,30,-1,'Serie'),(29,30,0,'20%'),(30,30,1,'60%'),(31,30,2,'100%'),(32,30,3,'Ram'),(33,32,-1,'Blau auf Weiß 1'),(34,32,0,'Blau auf Weiß 2'),(35,32,1,'Blau auf Weiß 3'),(36,32,2,'Gelb auf Blau'),(37,32,3,'Gelb auf Schwarz'),(38,17,-1,'Serie'),(39,17,0,'20%'),(40,17,1,'40%'),(41,17,2,'60%'),(42,17,3,'80%'),(43,17,4,'100%'),(44,15,-1,'Serie'),(45,15,0,'Jazz-Hupe 1'),(46,15,1,'Jazz-Hupe 2'),(47,15,2,'Jazz-Hupe 3'),(48,15,3,'Tonleiter Do'),(49,15,4,'Tonleiter Re'),(50,15,5,'Tonleiter Mi'),(51,15,6,'Tonleiter Fa'),(52,15,7,'Tonleiter Sol'),(53,15,8,'Tonleiter La'),(54,15,9,'Tonleiter Ti'),(55,15,10,'Tonleiter Do (hoch)'),(56,15,11,'Klassische Hupe 1'),(57,15,12,'Klassische Hupe 2'),(58,15,13,'Klassische Hupe 3'),(59,15,14,'Klassische Hupe 4'),(60,15,15,'Klassische Hupe 5'),(61,15,16,'Klassische Hupe 6'),(62,15,17,'Klassische Hupe 7'),(63,15,18,'Klassische Hupe 8'),(64,15,19,'Melodie-Hupe 1'),(65,15,20,'Melodie-Hupe 2'),(66,15,21,'Melodie-Hupe 3'),(67,15,22,'Melodie-Hupe 4'),(68,15,23,'Melodie-Hupe 5'),(69,15,24,'Traurige Posaune'),(70,15,25,'Jazz-Hupe, fortlaufend'),(71,15,26,'Klassische Hupe, fortlaufend 1'),(72,15,27,'Klassische Hupe, fortlaufend 2'),(73,15,28,'San Andreas, fortlaufend'),(74,15,29,'Liberty City, fortlaufend'),(75,15,30,'30'),(76,15,31,'31'),(77,15,32,'32'),(78,15,33,'33'),(79,15,34,'34'),(80,15,35,'35'),(81,15,36,'36'),(82,15,37,'37'),(83,15,38,'38'),(84,15,39,'39'),(85,15,40,'40'),(86,15,41,'41'),(87,15,42,'42'),(88,15,43,'43'),(89,15,44,'44'),(90,15,45,'45'),(91,15,46,'46'),(92,15,47,'47'),(93,15,48,'48'),(94,15,49,'49'),(95,15,50,'50'),(96,15,51,'51'),(97,15,52,'52'),(98,15,53,'53'),(99,15,54,'54'),(100,15,55,'55'),(101,15,56,'56');
/*!40000 ALTER TABLE `configvehicleuniversalmods` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:54
