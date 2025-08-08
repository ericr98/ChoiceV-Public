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
-- Table structure for table `confighairstyles`
--

DROP TABLE IF EXISTS `confighairstyles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `confighairstyles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `gtaId` int NOT NULL,
  `gender` varchar(1) NOT NULL,
  `level` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `isModded` tinyint(1) NOT NULL,
  `helmetReplacement` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_confighairstyles_helmentReplacement_idx` (`helmetReplacement`),
  CONSTRAINT `fk_confighairstyles_helmentReplacement` FOREIGN KEY (`helmetReplacement`) REFERENCES `confighairstyles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=148 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `confighairstyles`
--

LOCK TABLES `confighairstyles` WRITE;
/*!40000 ALTER TABLE `confighairstyles` DISABLE KEYS */;
INSERT INTO `confighairstyles` VALUES (4,0,'M',0,'Glatze',0,NULL),(7,1,'M',1,'Kurzhaarschnitt 5mm',0,NULL),(8,73,'M',1,'Topschnitt 5mm',0,NULL),(9,12,'M',2,'20mm Frisur',0,NULL),(10,72,'M',2,'Oben 30mm',0,NULL),(11,3,'M',2,'Undercut-Seiten 0mm',0,NULL),(12,21,'M',3,'Undercut Seitenscheitel',0,NULL),(13,30,'M',3,'Afro-Flattop',0,NULL),(14,41,'M',7,'Slick-Back',1,29),(15,4,'M',3,'Buisness Man',0,NULL),(16,5,'M',3,'Zerzaust',0,NULL),(17,33,'M',4,'Sidecut',0,NULL),(18,34,'M',3,'Irokesenschnitt',0,NULL),(19,6,'M',4,'wilder Irokese',0,NULL),(20,32,'M',4,'Undercut lang',0,NULL),(21,2,'M',5,'Facon zerzaust',0,NULL),(22,70,'M',4,'Der Stefano',0,NULL),(23,19,'M',4,'The Professional',0,NULL),(24,18,'M',4,'Seitenscheitel',0,NULL),(25,11,'M',4,'Der Bieber',0,NULL),(26,10,'M',4,'The Crew',0,NULL),(27,13,'M',5,'Der Bäcker',0,NULL),(28,31,'M',4,'Der Pompadour',0,NULL),(29,9,'M',5,'Der Gage',0,NULL),(30,36,'M',4,'Der John Lennon',0,NULL),(31,24,'M',5,'Cornrows 1',0,NULL),(32,25,'M',5,'Cornrows 2',0,NULL),(33,28,'M',5,'Cornrows 5',0,NULL),(34,26,'M',5,'Cornrows 3',0,NULL),(35,29,'M',5,'Cornrows 6',0,NULL),(36,27,'M',5,'Cornrows 4',0,NULL),(37,8,'M',6,'Cornrows dick',0,NULL),(38,14,'M',7,'Dreadlocks',0,NULL),(39,15,'M',7,'Der Snape',0,NULL),(40,20,'M',7,'Hockey Cut',0,NULL),(41,74,'M',8,'Der Elvis',0,NULL),(42,17,'M',8,'Langhaar',0,NULL),(43,16,'M',8,'Long Layered',0,NULL),(44,22,'M',8,'Fokuhila',0,NULL),(45,7,'M',7,'Pferdeschwanz',0,NULL),(47,38,'M',9,'Lange Haare',1,42),(48,39,'M',2,'Der Larry',1,NULL),(49,40,'M',8,'Der Afro',1,38),(51,42,'M',5,'Der Ken',1,63),(52,43,'M',8,'Langhaar Pferdeschwanz',0,NULL),(53,44,'M',8,'Boygroup',1,27),(54,45,'M',6,'Iro-Dreadlocks',1,38),(55,46,'M',8,'Der Jesus',1,42),(56,47,'M',3,'Voll hinten',1,12),(57,48,'M',4,'Verzweiflung',1,12),(58,49,'M',6,'Rough Day',1,29),(59,50,'M',4,'Das Schmieröl',0,NULL),(60,51,'M',4,'Der Undertopf',1,28),(61,52,'M',4,'Mittel zurückgegelt',1,28),(62,53,'M',6,'Short Quiff',1,27),(63,54,'M',6,'Knightrider',0,NULL),(64,0,'F',0,'Glatze',0,NULL),(65,1,'F',3,'Karen',0,NULL),(66,2,'F',5,'Der gestufter Bob',0,NULL),(67,3,'F',6,'Schulmädchen Zöpfe',0,NULL),(68,4,'F',6,'Pferdeschwanz',0,NULL),(69,5,'F',4,'geflochtener Iro',0,NULL),(70,6,'F',5,'Braids Ponytail',0,NULL),(71,7,'F',5,'Bob',0,NULL),(72,8,'F',3,'Modischer Pixy',0,NULL),(73,9,'F',6,'Karen long',0,NULL),(74,10,'F',4,'Langer Bob',0,NULL),(75,11,'F',6,'Lockere Olga',0,NULL),(76,12,'F',2,'Pixy Schnitt',0,NULL),(77,13,'F',4,'Undercut',0,NULL),(78,14,'F',5,'Dutt',0,NULL),(79,15,'F',4,'Alice',0,NULL),(80,16,'F',5,'Pin-Up-Girl',0,NULL),(81,17,'F',6,'Zerzauster Dutt',0,NULL),(82,18,'F',5,'Feder Haarband',0,NULL),(83,19,'F',6,'Kleiner Dutt',0,NULL),(84,20,'F',2,'Ashley',0,NULL),(85,21,'F',4,'Charleston Frisur',0,NULL),(86,22,'F',4,'Braid-Dutt',0,NULL),(87,23,'F',5,'Kontrollverlust',0,NULL),(88,24,'F',7,'Isabella',1,68),(89,25,'F',3,'Pinched-Cornrows',0,NULL),(90,26,'F',3,'Leaf-Cornrows',0,NULL),(91,27,'F',3,'Zick-Zack Cornrows',0,NULL),(92,28,'F',5,'Pigtail-Bangs',0,NULL),(93,29,'F',3,'Wave-Braids',0,NULL),(94,30,'F',3,'Coil-Braids',0,NULL),(95,31,'F',6,'Zimtschnecke',0,NULL),(96,32,'F',4,'Locker-zurückgekämmt',0,NULL),(97,33,'F',3,'Doppelter Sidecut',0,NULL),(98,34,'F',3,'Gekämmter-Undercut',0,NULL),(99,35,'F',2,'Irokese',0,NULL),(100,36,'F',5,'Boxer-Braids (Bandana)',0,NULL),(101,37,'F',7,'Sophia',1,68),(102,38,'F',4,'Miss Snape',0,NULL),(103,39,'F',7,'Lange wellige Haare',1,68),(104,40,'F',8,'Space Buns long',1,68),(105,41,'F',7,'Halber Boxer Braids',1,67),(106,42,'F',7,'Nach vorne Gelegt',1,68),(107,43,'F',7,'Lang mit Zöpfen',1,67),(108,44,'F',7,'Wasserfall',1,68),(109,45,'F',8,'Boxer Braids Long',1,67),(110,46,'F',8,'Rapunzel',1,68),(111,47,'F',7,'Space Buns',1,68),(113,49,'F',6,'Brav und Unschuldig',1,68),(114,50,'F',7,'Ariana',1,68),(115,51,'F',8,'Wavy',1,68),(118,54,'F',7,'Boxer Braids kurz',1,67),(119,55,'F',6,'Curly',1,68),(120,56,'F',9,'Lang Glatt',0,68),(121,57,'F',6,'Gepflegt mit Strähne',1,15),(123,59,'F',8,'hoher Zopf',1,68),(124,60,'F',6,'Space Buns mittel',1,15),(125,61,'F',5,'Business',1,15),(126,62,'F',6,'Buisness Gegenwind',1,15),(127,63,'F',8,'Lockig Lang',1,68),(128,64,'F',8,'Die Peitsche',1,67),(129,65,'F',8,'Ballkönigin',1,68),(130,66,'F',7,'Dutt mit Strähnen',1,68),(131,67,'F',8,'gepflegt Lang',1,68),(132,68,'F',8,'Takelage',1,67),(133,69,'F',4,'Helena',0,NULL),(134,70,'F',7,'Verwirrte Fehlentscheidung',1,68),(135,71,'F',5,'Sweet',1,15),(136,72,'F',7,'Bewusste Fehlentscheidung',1,68),(137,73,'F',8,'Piraten Zopf',1,67),(139,71,'M',4,'Slicked Back',0,NULL),(140,80,'F',2,'Kurzhaar-Welle',0,NULL),(141,81,'F',4,'Langer Bob (über Ohr)',0,NULL),(142,79,'F',3,'Afro Long',0,NULL),(143,58,'F',8,'Kreativ',1,68),(144,83,'F',1,'Buzz-Cut',0,NULL),(145,76,'M',4,'Man-Bun',0,NULL),(147,77,'M',3,'Platt-Cut',0,NULL);
/*!40000 ALTER TABLE `confighairstyles` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:55
