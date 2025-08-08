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
-- Table structure for table `configphonebookentries`
--

DROP TABLE IF EXISTS `configphonebookentries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configphonebookentries` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `number` bigint NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configphonebookentries`
--

LOCK TABLES `configphonebookentries` WRITE;
/*!40000 ALTER TABLE `configphonebookentries` DISABLE KEYS */;
INSERT INTO `configphonebookentries` VALUES (7,'Emergency',911),(12,'Los Santos Sheriff Department',930),(13,'Los Santos Police Department',920),(14,'Los Santos Fire Department',951),(15,'Saint Roper Medical Department',940),(16,'Department of Justice',580),(17,'City Administration',161),(18,'Taxi & Traffic School',3239999),(19,'Skiver Constructions',3230815),(20,'Praxis Dr. Goodman',3231122),(21,'L.S. Logistics',3230920),(22,'A.C.L.S.',3236969),(23,'Department of Sanitation',3232222),(24,'Daily Globe',3237291),(25,'City Bike Shop',3231899),(26,'Magic Moon',3230666),(27,'Premiuim Deluxe Motorsport',3231818),(28,'Hookies',3230200),(29,'San Andreas Insurance Company',3231904),(30,'Kanzlei Vega & Blair',3234455),(31,'Kurant Bar',3231133),(32,'Hill Valley Community Curch',3235454),(33,'Style & Perfomence Society',3231337),(34,'Mediterraneo Italian Cuccina',3239065);
/*!40000 ALTER TABLE `configphonebookentries` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:09:33
