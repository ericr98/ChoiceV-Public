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
-- Table structure for table `configinteriorentitysetspots`
--

DROP TABLE IF EXISTS `configinteriorentitysetspots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configinteriorentitysetspots` (
  `id` int NOT NULL AUTO_INCREMENT,
  `iplId` int NOT NULL,
  `displayName` varchar(45) NOT NULL,
  `interactSpotString` text NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_configinteriorentitysets_interiorId_idx` (`iplId`),
  CONSTRAINT `fk_configinteriorentitysets_interiorId` FOREIGN KEY (`iplId`) REFERENCES `configipls` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configinteriorentitysetspots`
--

LOCK TABLES `configinteriorentitysetspots` WRITE;
/*!40000 ALTER TABLE `configinteriorentitysetspots` DISABLE KEYS */;
INSERT INTO `configinteriorentitysetspots` VALUES (2,39,'Besprechung','{\"X\":-436.97626,\"Y\":5993.5576,\"Z\":30.706177}#-1.9658613#2.2518775#46.427124#true#false#true'),(3,40,'Eingangsscreen','{\"X\":-206.26212,\"Y\":-39.04976,\"Z\":49.679077}#1.5279915#1.138529#70.11575#true#false#true'),(4,41,'KRNT-Barstühle','{\"X\":374.26813,\"Y\":-1079.4989,\"Z\":29.46521}#0.9399415#2.0226238#0.0#true#false#true'),(5,38,'Vorhänge Süd','{\"X\":246.25055,\"Y\":-1114.2393,\"Z\":29.279907}#2.0226238#0.9399415#0.0#true#false#true'),(6,38,'Vorhänge Nord','{\"X\":247.01538,\"Y\":-1075.3783,\"Z\":29.279907}#2.0226238#1.0752767#0.0#true#false#true'),(7,38,'Security Setup','{\"X\":257.08597,\"Y\":-1093.674,\"Z\":29.279907}#3.3759766#2.0226238#0.0#true#false#true'),(8,20,'PDM Einrichtung','{\"X\":0,\"Y\":0,\"Z\":0}#0#0#0.0#true#false#true'),(9,20,'PDM Garagentor','{\"X\":-33.010925,\"Y\":-1086.0891,\"Z\":26.415405}#1.2688887#1.9141794#68.60507#true#false#true'),(10,46,'Neon Steuerung','{\"X\":948.52747,\"Y\":-1483.701,\"Z\":30.45935}#0.9399415#1.3459474#0.0#true#false#true'),(11,57,'Stühle Position','{\"X\":2514.5276,\"Y\":4101.864,\"Z\":35.581665}#1.3269265#0.9872042#62.339752#true#false#true'),(12,78,'Michaels House Gegenstände','{\"X\":0,\"Y\":0,\"Z\":0}#0#0#0.0#true#false#true');
/*!40000 ALTER TABLE `configinteriorentitysetspots` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:16:34
