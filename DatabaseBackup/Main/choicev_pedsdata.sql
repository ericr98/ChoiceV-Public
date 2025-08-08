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
-- Table structure for table `pedsdata`
--

DROP TABLE IF EXISTS `pedsdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pedsdata` (
  `pedId` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `value` text NOT NULL,
  PRIMARY KEY (`pedId`,`name`),
  CONSTRAINT `fk_configpedsdata_pedId` FOREIGN KEY (`pedId`) REFERENCES `configpeds` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedsdata`
--

LOCK TABLES `pedsdata` WRITE;
/*!40000 ALTER TABLE `pedsdata` DISABLE KEYS */;
INSERT INTO `pedsdata` VALUES (18,'StealInteractions','\"[]\"'),(19,'StealInteractions','\"[{\\\"PlayerId\\\":1149,\\\"Date\\\":\\\"2024-11-21T11:40:53.3565142+01:00\\\",\\\"Amount\\\":11,\\\"HasBeenDispatched\\\":true}]\"'),(22,'Active','true'),(25,'Active','false'),(26,'Active','false'),(236,'StealInteractions','\"[]\"'),(237,'StealInteractions','\"[]\"'),(238,'StealInteractions','\"[]\"'),(239,'StealInteractions','\"[]\"'),(243,'StealInteractions','\"[]\"'),(244,'Active','false'),(244,'StealInteractions','\"[]\"'),(245,'StealInteractions','\"[]\"'),(247,'StealInteractions','\"[]\"'),(249,'StealInteractions','\"[]\"'),(250,'StealInteractions','\"[]\"'),(253,'StealInteractions','\"[{\\\"PlayerId\\\":960,\\\"Date\\\":\\\"2024-11-18T18:57:57.3692669+01:00\\\",\\\"Amount\\\":1,\\\"HasBeenDispatched\\\":true}]\"'),(254,'StealInteractions','\"[]\"'),(256,'Active','true'),(268,'LastRobbed','\"2024-10-20T16:25:58.0733141+02:00\"'),(280,'StealInteractions','\"[]\"'),(281,'StealInteractions','\"[]\"'),(286,'Active','false'),(287,'Active','true'),(288,'Active','true'),(289,'Active','true'),(293,'Active','false'),(298,'Active','false'),(299,'Active','true'),(300,'Active','true'),(305,'Active','true'),(314,'Active','false'),(320,'Active','false'),(324,'StealInteractions','\"[]\"'),(338,'StealInteractions','\"[]\"'),(14764,'DealerNPCModuleInventory','52856'),(14766,'DealerNPCModuleInventory','52858'),(14767,'DealerNPCModuleInventory','52859'),(14768,'DealerNPCModuleInventory','52860'),(14769,'DealerNPCModuleInventory','52861'),(14770,'DealerNPCModuleInventory','52862'),(14771,'DealerNPCModuleInventory','52863'),(14772,'DealerNPCModuleInventory','52864'),(14773,'DealerNPCModuleInventory','52865'),(14774,'DealerNPCModuleInventory','52866'),(14775,'DealerNPCModuleInventory','52867'),(14776,'DealerNPCModuleInventory','52868'),(14777,'DealerNPCModuleInventory','52869'),(14778,'DealerNPCModuleInventory','52870');
/*!40000 ALTER TABLE `pedsdata` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:16:49
