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
-- Table structure for table `configinjurytreatments`
--

DROP TABLE IF EXISTS `configinjurytreatments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configinjurytreatments` (
  `identifier` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  `name` varchar(100) NOT NULL DEFAULT '',
  PRIMARY KEY (`identifier`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configinjurytreatments`
--

LOCK TABLES `configinjurytreatments` WRITE;
/*!40000 ALTER TABLE `configinjurytreatments` DISABLE KEYS */;
INSERT INTO `configinjurytreatments` VALUES ('BRAIN_PUNCTURE','Durchstich mit Hirnverletzung'),('BRAIN_SHOT','Durchschusswunde mit Hirnverletzung'),('BRUISE','Prellung'),('BURNING','Verbrennung'),('CONCUSSION','Gehirnerschütterung'),('DEEP_BRUISE','Tiefer Bluterguss'),('DEEP_CUT','Tiefer Schnitt'),('FRACTURE','Fraktur'),('FRACTURE_SHOT','Durchschuss mit Schussfraktur (4-6) (OP)'),('GAPING_CUT','Klaffende Schnittwunde'),('INFLAMMATION','Entzündung'),('LACERATION','Platzwunde'),('MUSCLE_STR','Muskel-Zerrung'),('NONBRAIN_GRAZE','Steckschuss/Ringelschuss ohne Hirnverletung'),('ORGANS_INJURY','Verletzung innerer Organe'),('ORGANS_SHOT','Durchschusswunde mit Verletzung innerer Organe'),('PUNCTURE_WOUND','Durchstichwunde'),('SPRAIN','Verstauchung'),('SUPERFICIAL_WOUND','Oberflächlicher Schnitt/Streifschuss'),('THROUGH_WOUND','Durchschusswunde'),('VASCULAR_PUNCTURE','Durchstich mit Gefäßverletzung'),('VASCULAR_SHOT','Durchschusswunde mit Gefäßverletzung');
/*!40000 ALTER TABLE `configinjurytreatments` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:14:25
