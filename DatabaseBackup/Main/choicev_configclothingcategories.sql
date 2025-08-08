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
-- Table structure for table `configclothingcategories`
--

DROP TABLE IF EXISTS `configclothingcategories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configclothingcategories` (
  `id` int NOT NULL,
  `slotId` int NOT NULL,
  `Name` varchar(45) NOT NULL,
  `hideSpace` float NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configclothingcategories`
--

LOCK TABLES `configclothingcategories` WRITE;
/*!40000 ALTER TABLE `configclothingcategories` DISABLE KEYS */;
INSERT INTO `configclothingcategories` VALUES (1,1,'Tiermasken',0),(2,1,'Bandana',0),(3,1,'Sturmmasken',0),(4,1,'Horrormasken',0),(5,1,'Sonstige',0),(6,4,'Kurze Hosen',0.5),(7,4,'Lange Hosen',1),(8,4,'Jogginghosen',1),(9,4,'Roecke',0.5),(10,4,'Arbeitshosen',1),(11,4,'Anzugshosen',1),(12,4,'Unterwäsche',0),(13,4,'Badekleidung',0),(14,4,'Kostümhosen',1),(15,6,'Turnschuhe',0.25),(16,6,'Elegante Schuhe',0.25),(17,6,'Hohe Schuhe',0.5),(18,6,'Kostümschuhe',0.25),(19,6,'Arbeitsschuhe',0.5),(20,6,'Stiefel',0.5),(21,6,'Mokassins',0),(22,6,'Flip-Flops',0),(23,7,'Krawatte',0.05),(24,7,'Fliege',0.05),(25,7,'Ohrring',0),(26,7,'Kette',0),(27,7,'Schals',0),(28,7,'Armbänder',0),(29,7,'Sonstiges',0),(30,8,'Kurzarmshirts',0.25),(31,8,'Langarmshirts',0.5),(32,8,'Tops',0.5),(33,11,'Kurzarmshirts',0.25),(34,11,'Langarmshirts',0.5),(35,11,'Tops',0.5),(36,11,'Kleider',0.5),(37,11,'Jacken',1),(38,11,'Mäntel',1),(39,11,'Sakkos',1),(40,11,'Kostüme',1),(41,11,'Oversize',0.5),(42,11,'Badekleidung',0),(43,11,'Trainingskleidung',0.25),(44,11,'Blusen/Hemden',0.25),(45,9,'Schutzwesten',0.25),(46,9,'Warnweste',0.25),(47,9,'Holster',0),(48,9,'Funkgerät',0),(49,10,'Aufnäher',0),(50,10,'Holster',0),(51,8,'Gürtel',0),(52,0,'Helm',0),(53,0,'Hut',0),(54,8,'Weste',0),(55,9,'Umhängetasche',1),(56,9,'Collage Jacke',1),(57,5,'Atemschutzgerät',0),(58,11,'Kapuzenpullover',0.5),(59,5,'Kutte',0.5),(60,5,'Klettergurt',0),(61,11,'Bandage',0);
/*!40000 ALTER TABLE `configclothingcategories` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:02
