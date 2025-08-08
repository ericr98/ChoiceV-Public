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
-- Table structure for table `worldprocessmachinesinventories`
--

DROP TABLE IF EXISTS `worldprocessmachinesinventories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `worldprocessmachinesinventories` (
  `machineId` int NOT NULL,
  `identifier` varchar(45) NOT NULL,
  `inventoryId` int NOT NULL,
  PRIMARY KEY (`machineId`,`identifier`),
  KEY `fk_worldprocessmachinesinventories_inventoryId_idx` (`inventoryId`),
  CONSTRAINT `fk_worldprocessmachinesinventories_inventoryId` FOREIGN KEY (`inventoryId`) REFERENCES `inventories` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_worldprocessmachinesinventories_machineId` FOREIGN KEY (`machineId`) REFERENCES `worldprocessmachines` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `worldprocessmachinesinventories`
--

LOCK TABLES `worldprocessmachinesinventories` WRITE;
/*!40000 ALTER TABLE `worldprocessmachinesinventories` DISABLE KEYS */;
INSERT INTO `worldprocessmachinesinventories` VALUES (0,'GarbInv',26350),(0,'WassInv',26351),(0,'MessInv',26352),(0,'FlakeInv',26353),(0,'RestInv',26354),(2,'PapierInv',27140),(2,'TinteInv',27141),(2,'PlattenInv',27142),(2,'ZeitungsInv',27143),(4,'BagInv',27520),(4,'TrashInv',27521),(5,'GarbInv',33394),(5,'WassInv',33395),(5,'MessInv',33396),(5,'FlakeInv',33397),(5,'RestInv',33398);
/*!40000 ALTER TABLE `worldprocessmachinesinventories` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:17:05
