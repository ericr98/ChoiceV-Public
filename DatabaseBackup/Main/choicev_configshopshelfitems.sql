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
-- Table structure for table `configshopshelfitems`
--

DROP TABLE IF EXISTS `configshopshelfitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configshopshelfitems` (
  `shelfId` int NOT NULL,
  `itemId` int NOT NULL,
  PRIMARY KEY (`shelfId`,`itemId`),
  KEY `fk_configshopshelfitems_itemId_idx` (`itemId`),
  CONSTRAINT `fk_configshopshelfitems_itemId` FOREIGN KEY (`itemId`) REFERENCES `configitems` (`configItemId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_configshopshelfitems_shelfId` FOREIGN KEY (`shelfId`) REFERENCES `configshopshelfs` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configshopshelfitems`
--

LOCK TABLES `configshopshelfitems` WRITE;
/*!40000 ALTER TABLE `configshopshelfitems` DISABLE KEYS */;
INSERT INTO `configshopshelfitems` VALUES (25,14),(30,23),(30,91),(30,93),(30,94),(6,176),(7,176),(9,176),(18,176),(6,177),(7,177),(9,177),(18,177),(6,178),(7,178),(9,178),(18,178),(23,198),(23,199),(23,200),(23,201),(6,202),(7,202),(9,202),(18,202),(18,203),(18,204),(18,205),(18,206),(18,207),(18,208),(24,211),(24,212),(24,213),(24,214),(24,215),(24,216),(24,217),(24,218),(25,219),(25,220),(25,221),(25,222),(23,226),(23,227),(23,228),(12,231),(13,231),(16,231),(29,231),(12,232),(13,232),(16,232),(17,232),(12,233),(13,233),(16,233),(29,233),(12,234),(17,234),(12,235),(19,235),(12,236),(19,236),(29,236),(12,237),(17,237),(19,237),(29,237),(12,238),(29,238),(10,239),(13,239),(16,239),(28,239),(29,239),(10,240),(13,240),(16,240),(19,240),(27,240),(29,240),(10,241),(13,241),(16,241),(27,241),(29,241),(17,242),(29,242),(29,243),(17,245),(29,245),(17,246),(29,246),(15,251),(15,253),(12,255),(15,255),(17,260),(3,269),(3,270),(9,271),(9,272),(11,306),(26,306),(11,307),(26,307),(11,308),(26,308),(8,314),(8,315),(30,321),(30,459),(15,484),(15,486),(6,519),(6,520),(19,526),(29,526),(10,527),(27,527),(29,527),(12,528),(19,528),(29,528),(29,538),(14,539),(29,539),(29,541),(14,543),(29,543),(29,544),(14,545),(29,545);
/*!40000 ALTER TABLE `configshopshelfitems` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:16:07
