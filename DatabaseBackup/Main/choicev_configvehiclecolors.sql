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
-- Table structure for table `configvehiclecolors`
--

DROP TABLE IF EXISTS `configvehiclecolors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configvehiclecolors` (
  `gtaId` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `type` varchar(45) NOT NULL,
  PRIMARY KEY (`gtaId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configvehiclecolors`
--

LOCK TABLES `configvehiclecolors` WRITE;
/*!40000 ALTER TABLE `configvehiclecolors` DISABLE KEYS */;
INSERT INTO `configvehiclecolors` VALUES (-1,'Keine Lackierung','None'),(0,'Schwarz','Metallic'),(1,'Graphitschwarz','Metallic'),(2,'Stahlschwarz','Metallic'),(3,'Dunkelsilber','Metallic'),(4,'Silber','Metallic'),(5,'Blausilber','Metallic'),(6,'Stahlgrau','Metallic'),(7,'Schattensilber','Metallic'),(8,'Steinsilber','Metallic'),(9,'MitternachtsSilber','Metallic'),(10,'Waffenstahl','Metallic'),(11,'Anthrazitgrau','Metallic'),(12,'Schwarz','Matt'),(13,'Grau','Matt'),(14,'Hellgrau','Matt'),(15,'Schwarz','Util'),(16,'Polyschwarz','Util'),(17,'Dunkelsilber','Util'),(18,'Silber','Util'),(19,'Waffenstahl','Util'),(20,'Schattenssilber','Util'),(21,'Schwarz','Worn'),(22,'Graphit','Worn'),(23,'Silbergrau','Worn'),(24,'Silber','Worn'),(25,'Silberblau','Worn'),(26,'Schattensilber','Worn'),(27,'Rot','Metallic'),(28,'Torinorot','Metallic'),(29,'Formularot','Metallic'),(30,'Blazerot','Metallic'),(31,'Gracefulrot','Metallic'),(32,'Garnetrot','Metallic'),(33,'Desertrot','Metallic'),(34,'Cabernetrot','Metallic'),(35,'Candyrot','Metallic'),(36,'Sunriseorange','Metallic'),(37,'Classicgold','Metallic'),(38,'Orange','Metallic'),(39,'Rot','Matt'),(40,'Dunkelrot','Matt'),(41,'Orange','Matt'),(42,'Gelb','Matt'),(43,'Rot','Util'),(44,'Hellrot','Util'),(45,'Granatrot','Util'),(46,'Rot','Worn'),(47,'Goldrot','Worn'),(48,'Dunkelrot','Worn'),(49,'Dunkelgrün','Metallic'),(50,'Renngrün','Metallic'),(51,'Seegrün','Metallic'),(52,'Olivgrün','Metallic'),(53,'Grün','Metallic'),(54,'Gasolineblaugrün','Metallic'),(55,'Limettengrün','Matt'),(56,'Dunkelgrün','Util'),(57,'Grün','Util'),(58,'Dunkelgrün','Worn'),(59,'Grün','Worn'),(60,'Seegrün','Worn'),(61,'Mitternachtsblau','Metallic'),(62,'Dunkelblau','Metallic'),(63,'Sachsenblau','Metallic'),(64,'Blau','Metallic'),(65,'Marineblau','Metallic'),(66,'Hafenblau','Metallic'),(67,'Diamantblau','Metallic'),(68,'Surferblau','Metallic'),(69,'Nauticalblau','Metallic'),(70,'Hellblau','Metallic'),(71,'Aubergine','Metallic'),(72,'Spinnakerblau','Metallic'),(73,'Ultrablau','Metallic'),(74,'Pastellblau','Metallic'),(75,'Dunkelblau','Util'),(76,'Mitternachtsblau','Util'),(77,'Blau','Util'),(78,'Seeschaumblau','Util'),(79,'Blitzblau','Util'),(80,'Maui Polyblau','Util'),(81,'Hellblau','Util'),(82,'Dunkelblau','Matt'),(83,'Blau','Matt'),(84,'Mitternachtsblau','Matt'),(85,'Dunkelblau','Worn'),(86,'Blau','Worn'),(87,'Hellblau','Worn'),(88,'Taxigelb','Metallic'),(89,'Renngelb','Metallic'),(90,'Bronze','Metallic'),(91,'Vogelgelb','Metallic'),(92,'Limette','Metallic'),(93,'Champagner','Metallic'),(94,'Pueblobeige','Metallic'),(95,'Dunkles Elfenbein','Metallic'),(96,'Schokobraun','Metallic'),(97,'Goldbraun','Metallic'),(98,'Hellbraun','Metallic'),(99,'Stroh-Beige','Metallic'),(100,'Moosbraun','Metallic'),(101,'Biston-Braun','Metallic'),(102,'Buche','Metallic'),(103,'Buche-Dunkel','Metallic'),(104,'Choco','Metallic'),(105,'Strand-Sand','Metallic'),(106,'Sonnengebleichter-Sand','Metallic'),(107,'Cream','Metallic'),(108,'Braun','Util'),(109,'Mittelbraun','Util'),(110,'Hellbraun','Util'),(111,'Weiß','Metallic'),(112,'Eisweiß','Metallic'),(113,'Honigbeige','Worn'),(114,'Braun','Worn'),(115,'Dunkelbraun','Worn'),(116,'Strohbeige','Worn'),(117,'Stahl','Brushed'),(118,'Schwarzer Stahl','Brushed'),(119,'Aluminium','Brushed'),(120,'Chrome','Spezial'),(121,'Gebrochenweiß','Worn'),(122,'Gebrochenweiß','Util'),(123,'Orange','Worn'),(124,'Hellorange','Worn'),(126,'Taxigelb','Worn'),(128,'Grün','Matt'),(129,'Braun','Matt'),(130,'Orange','Worn'),(131,'Weiß','Matt'),(132,'Weiß','Worn'),(133,'Olivearmeegrün','Worn'),(134,'Pures Weiß','Spezial'),(135,'Hot Pink','Spezial'),(136,'Lachs','Spezial'),(138,'Hellorange','Metallic'),(141,'Schwarzblau','Metallic'),(142,'Schwarzlila','Metallic'),(143,'Schwarzrot','Metallic'),(144,'Jagdgrün','Spezial'),(145,'Lila','Metallic'),(146,'V Dunkelblau','Spezial'),(148,'Lila','Matt'),(149,'Dunkellila','Matt'),(150,'Lavarot','Metallic'),(151,'Waldgrün','Matt'),(152,'Olivgrün','Matt'),(153,'Wüstenbraun','Matt'),(154,'Wüstenlohfarbe','Matt'),(155,'Laubgrün','Matt'),(157,'Epislon Blau','Spezial'),(158,'Pures Gold','Spezial'),(159,'Gold','Brushed');
/*!40000 ALTER TABLE `configvehiclecolors` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:12:19
