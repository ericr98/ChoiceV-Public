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
-- Table structure for table `confighaircolors`
--

DROP TABLE IF EXISTS `confighaircolors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighaircolors` (
  `id` int DEFAULT NULL,
  `name` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighaircolors`
--

LOCK TABLES `confighaircolors` WRITE;
/*!40000 ALTER TABLE `confighaircolors` DISABLE KEYS */;
INSERT INTO `confighaircolors` VALUES (0,'Schwarz'),(1,'Grau'),(2,'Grau'),(3,'Braun'),(4,'Braun'),(5,'Braun'),(6,'Braun'),(7,'Braun'),(8,'Blond'),(9,'Blond'),(10,'Blond'),(11,'Blond'),(12,'Blond'),(13,'Blond'),(14,'Blond'),(15,'Blond'),(16,'Blond'),(17,'Rotbraun'),(18,'Rotbraun'),(19,'Rot'),(20,'Rot'),(21,'Rot'),(22,'Orange'),(23,'Orange'),(24,'Orange'),(25,'Orange'),(26,'Grau'),(27,'Grau'),(28,'Grau'),(29,'Weiß'),(30,'Violet'),(31,'Violet'),(32,'Pink'),(33,'Pink'),(34,'Pink'),(35,'Pink'),(36,'Türkis'),(37,'Türkis'),(38,'Blau'),(39,'Grün'),(40,'Grün'),(41,'Grün'),(43,'Grün'),(44,'Grün'),(45,'Gelb'),(46,'Gelb'),(47,'Orange'),(48,'Orange'),(49,'Orange'),(50,'Orange'),(51,'Orange'),(52,'Rot'),(53,'Rot'),(54,'Braun'),(55,'Braun'),(56,'Braun'),(57,'Braun'),(58,'Braun'),(59,'Braun'),(60,'Braun'),(61,'Schwarz'),(62,'Blond'),(63,'Blond');
/*!40000 ALTER TABLE `confighaircolors` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:16:12
