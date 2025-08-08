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
-- Table structure for table `configitemsitemcontainerinfo`
--

DROP TABLE IF EXISTS `configitemsitemcontainerinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configitemsitemcontainerinfo` (
  `configItemId` int NOT NULL,
  `subConfigItemId` int NOT NULL,
  `subItemAmount` int NOT NULL,
  PRIMARY KEY (`configItemId`,`subConfigItemId`),
  KEY `fk_configitemsitemcontainerinfo_subconfigItemIds_idx` (`subConfigItemId`),
  CONSTRAINT `fk_configitemsitemcontainerinfo_configItemId` FOREIGN KEY (`configItemId`) REFERENCES `configitems` (`configItemId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_configitemsitemcontainerinfo_subconfigItemIds` FOREIGN KEY (`subConfigItemId`) REFERENCES `configitems` (`configItemId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configitemsitemcontainerinfo`
--

LOCK TABLES `configitemsitemcontainerinfo` WRITE;
/*!40000 ALTER TABLE `configitemsitemcontainerinfo` DISABLE KEYS */;
INSERT INTO `configitemsitemcontainerinfo` VALUES (2,0,5),(18,17,5),(179,182,4),(179,183,4),(179,189,1),(188,174,12),(189,184,15),(190,185,8),(191,186,4),(192,187,8),(239,231,6),(240,232,6),(241,233,6),(242,234,12),(243,235,12),(244,236,12),(245,237,12),(246,238,12),(252,251,6),(254,253,6),(256,255,6),(260,257,6),(261,258,6),(262,259,6),(263,268,20),(264,268,20),(265,268,20),(266,268,20),(267,268,20),(295,14,24),(296,219,24),(297,220,24),(298,221,24),(299,222,24),(309,306,24),(310,307,24),(311,308,24),(312,229,24),(313,230,12),(435,258,10),(444,23,4),(445,25,15),(446,30,8),(447,36,20),(448,447,20),(449,52,10),(450,53,5),(451,59,6),(452,451,20),(453,62,15),(454,68,6),(455,70,12),(456,71,12),(457,73,8),(458,74,8),(459,92,6),(460,156,30),(461,158,10),(462,162,20),(463,173,5),(464,175,15),(465,191,10),(466,189,10),(467,190,10),(468,192,12),(469,188,12),(470,346,12),(471,347,12),(472,348,12),(473,360,8),(477,350,5),(478,351,5),(479,227,12),(480,230,5),(483,482,10),(485,484,6),(487,466,6),(520,519,6),(527,526,6),(588,491,12),(604,603,50),(616,601,15),(617,602,30),(627,557,100),(644,641,6);
/*!40000 ALTER TABLE `configitemsitemcontainerinfo` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:13:44
