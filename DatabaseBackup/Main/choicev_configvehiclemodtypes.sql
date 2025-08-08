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
-- Table structure for table `configvehiclemodtypes`
--

DROP TABLE IF EXISTS `configvehiclemodtypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configvehiclemodtypes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `ModTypeIndex` int NOT NULL,
  `ModTypeName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `DisplayName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `IsUniversal` tinyint(1) NOT NULL DEFAULT '0',
  `OnlyMotorcycle` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configvehiclemodtypes`
--

LOCK TABLES `configvehiclemodtypes` WRITE;
/*!40000 ALTER TABLE `configvehiclemodtypes` DISABLE KEYS */;
INSERT INTO `configvehiclemodtypes` VALUES (1,0,'Spoilers','Spoiler',0,0),(2,1,'FrontBumper','Frontstoßstange',0,0),(3,2,'RearBumper','Heckstoßstange',0,0),(4,3,'SideSkirt','Seitenschweller',0,0),(5,4,'Exhaust','Auspuff',0,0),(6,5,'Frame','Rahmen',0,0),(7,6,'Grille','Kühlergrill',0,0),(8,7,'Hood','Motorhaube',0,0),(9,8,'Fender','Kotflügel',0,0),(10,9,'RightFender','Rechter Kotflügel',0,0),(11,10,'Roof','Dach',0,0),(12,11,'Engine','Motor',1,0),(13,12,'Brakes','Bremse',1,0),(14,13,'Transmission','Getriebe',1,0),(15,14,'Horns','Hupe',1,0),(16,15,'Suspension','Federung',1,0),(17,16,'Armor','Rüstung',1,0),(18,18,'Turbo','Turbo',1,0),(19,22,'Xenon','Licht',1,0),(20,23,'FrontWheels','Vorderräder',0,0),(21,24,'BackWheels','Hinterräder',0,1),(22,25,'PlateHolders','Kennzeichenhalter',0,0),(23,27,'TrimDesign','Trim-Design',0,0),(24,28,'Ornaments','Verzierungen',0,0),(25,30,'DialDesign','Zifferblatt-Design',0,0),(26,33,'SteeringWheel','Lenkrad',0,0),(27,34,'ShiftLever','Schalthebel',0,0),(28,35,'Plaques','Plaketten',0,0),(29,38,'Hydraulics','Hydraulik',0,0),(30,40,'Boost','Boost',1,0),(31,48,'Livery','Lackierung',0,0),(32,62,'Plate','Kennzeichen',1,0),(33,66,'Color1','Primärfarbe',0,0),(34,67,'Color2','Sekundärfarbe',0,0),(35,69,'WindowTint','Fenstertönung',1,0),(36,74,'DashboardColor','Dashboard-Farbe',0,0),(37,75,'TrimColor','Trim-Farbe',0,0);
/*!40000 ALTER TABLE `configvehiclemodtypes` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:00
