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
-- Table structure for table `configinjuries`
--

DROP TABLE IF EXISTS `configinjuries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configinjuries` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `bodyPart` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `treatmentCategory` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  `damageType` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `minSeverness` int NOT NULL,
  `maxSeverness` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_configinjuries_treatmentCategory_idx` (`treatmentCategory`),
  CONSTRAINT `fk_configinjuries_treatmentCategory` FOREIGN KEY (`treatmentCategory`) REFERENCES `configinjurytreatments` (`identifier`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=142 DEFAULT CHARSET=dec8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configinjuries`
--

LOCK TABLES `configinjuries` WRITE;
/*!40000 ALTER TABLE `configinjuries` DISABLE KEYS */;
INSERT INTO `configinjuries` VALUES (1,'Prellung','All','BRUISE','Dull',1,3),(2,'Platzwunde','All','LACERATION','Dull',2,4),(3,'Tiefer Bluterguss','All','DEEP_BRUISE','Dull',4,6),(4,'Verstauchung','Limbs','SPRAIN','Dull',1,3),(5,'Fraktur','All','FRACTURE','Dull',4,6),(6,'Muskel-Zerrung','Limbs','MUSCLE_STR','Dull',2,4),(7,'Verletzung innerer Organe (Stumpf)','Torso','ORGANS_INJURY','Dull',5,6),(8,'Gehirnerschütterung','Head','CONCUSSION','Dull',1,4),(9,'Glatter Schnitt','All','SUPERFICIAL_WOUND','Sting',1,3),(10,'Tiefer Schnitt','All','DEEP_CUT','Sting',3,4),(11,'Durchstichwunde','All','PUNCTURE_WOUND','Sting',4,6),(12,'Klaffende Schnittwunde','All','GAPING_CUT','Sting',3,4),(13,'Durchstich mit Gefäßverletzung','All','VASCULAR_PUNCTURE','Sting',4,6),(14,'Verletzung innerer Organe (Stich)','Torso','ORGANS_INJURY','Sting',5,6),(15,'Durchstich mit Hirnverletzung','Head','BRAIN_PUNCTURE','Sting',4,6),(16,'Streifschuss','All','SUPERFICIAL_WOUND','Shot',1,3),(17,'Steckschuss','All','NONBRAIN_GRAZE','Shot',3,4),(18,'Durchschusswunde','All','THROUGH_WOUND','Shot',4,6),(19,'Durchschuss mit Schussfraktur','All','FRACTURE_SHOT','Shot',4,6),(20,'Durchschuss mit Gefäßverletzung','All','VASCULAR_SHOT','Shot',4,6),(21,'Verletzung innerer Organe (Schuss)','Torso','ORGANS_INJURY','Shot',5,6),(22,'Ringelschuss ohne Hirnverletzung','Head','NONBRAIN_GRAZE','Shot',3,4),(23,'Ringelschuss mit Hirnverletzung','Head','BRAIN_SHOT','Shot',5,6),(24,'Entzündung','All','INFLAMMATION','Inflammation',1,6),(25,'Verbrennung','All','BURNING','Burning',1,6);
/*!40000 ALTER TABLE `configinjuries` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:09:32
