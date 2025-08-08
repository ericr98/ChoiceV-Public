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
-- Table structure for table `configgarages`
--

DROP TABLE IF EXISTS `configgarages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configgarages` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `position` varchar(255) NOT NULL,
  `rotation` float NOT NULL,
  `type` int NOT NULL,
  `ownerType` int NOT NULL,
  `ownerId` int NOT NULL,
  `slots` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configgarages`
--

LOCK TABLES `configgarages` WRITE;
/*!40000 ALTER TABLE `configgarages` DISABLE KEYS */;
INSERT INTO `configgarages` VALUES (2,'City Bikes Garage','{\"X\":-171.7978,\"Y\":-32.43956,\"Z\":51.212402}',-110.551,1,2,20,100),(4,'PDM_SHOWROOM','{\"X\":-7.89889,\"Y\":-1088.5846,\"Z\":25.66809}',76.5355,1,2,24,100),(5,'Parkhaus Rancho','{\"X\":343.26593,\"Y\":-1688.6505,\"Z\":31.515015}',-116.221,1,3,-1,99999),(6,'Garage AmmuNation','{\"X\":25.33187,\"Y\":-1099.9253,\"Z\":37.142822}',62.3623,1,3,-1,99999),(8,'Parkplatz Diamond Casino','{\"X\":886.33844,\"Y\":-0.32966995,\"Z\":77.75098}',144.567,1,3,-1,99999),(9,'Parkplatz ClintonAve','{\"X\":361.8857,\"Y\":298.25934,\"Z\":102.87402}',-8.50394,1,3,-1,99999),(11,'Parkplatz Aguja Street','{\"X\":-1184.8748,\"Y\":-1510.0747,\"Z\":3.6453857}',-51.0237,1,3,-1,99999),(12,'Parkplatz Sandy Shores','{\"X\":1525.1077,\"Y\":3773.077,\"Z\":33.503296}',-175.748,1,3,-1,99999),(14,'Garage New Empire Way','{\"X\":-997.978,\"Y\":-2609.1033,\"Z\":13.11499}',56.693,1,3,-1,99999),(15,'Parkhaus Burton Street','{\"X\":-308.98022,\"Y\":-58.153847,\"Z\":53.4198}',133.228,1,3,-1,99999),(16,'Firmengarage LSDOS','{\"X\":-440.87473,\"Y\":-1694.4923,\"Z\":18.10254}',155.906,1,2,18,50),(19,'Parkhaus Pillbox','{\"X\":-330.5011,\"Y\":-781.4374,\"Z\":32.96411}',45.3544,1,3,-1,99999),(20,'LSPD Garage','{\"X\":850.022,\"Y\":-1343.578,\"Z\":19.231445}',-85.0394,1,2,2,100),(22,'LSPD Impound','{\"X\":455.94727,\"Y\":-1145.2087,\"Z\":28.364136}',-178.583,1,2,2,99999),(23,'LSFD','{\"X\":1194.0527,\"Y\":-1473.6263,\"Z\":33.857178}',-87.8741,1,2,3,100),(25,'LSFD Paleto','{\"X\":-367.96484,\"Y\":6115.345,\"Z\":30.436646}',-34.0158,1,2,3,100),(26,'LSMD Garage','{\"X\":-351.69232,\"Y\":-239.65714,\"Z\":36.266724}',-36.8504,1,2,4,100),(27,'DOJ Garage','{\"X\":261.69232,\"Y\":-1108.3385,\"Z\":28.717896}',178.583,1,2,5,50),(29,'Sheriff Impound Garage','{\"X\":-434.0044,\"Y\":6158.848,\"Z\":30.470337}',-136.063,1,2,1,99999),(30,'Sheriff Garage','{\"X\":-358.58902,\"Y\":6061.688,\"Z\":30.487183}',53.8583,1,2,1,100),(32,'Garage Traffic School & Transportation Service','{\"X\":-281.12967,\"Y\":-888.32965,\"Z\":30.30188}',158.74,1,2,19,50),(33,'Daily Globe Garage','{\"X\":698.7956,\"Y\":216.52747,\"Z\":91.33191}',-150.236,1,2,10,30),(34,'Garage Paleto','{\"X\":83.828575,\"Y\":6420.5537,\"Z\":30.756836}',-36.8504,1,3,-1,99999),(35,'Garage Grapeseed','{\"X\":1707.8506,\"Y\":4791.9033,\"Z\":40.967773}',96.378,1,3,-1,99999),(36,'Garage Richman','{\"X\":-1667.9604,\"Y\":43.173626,\"Z\":61.928955}',116.221,1,3,-1,99999),(37,'Garage Hafen','{\"X\":797.23517,\"Y\":-2974.7869,\"Z\":5.010254}',104.882,1,3,-1,99999),(38,'Garage Stadtverwaltung','{\"X\":-1276.9187,\"Y\":-604.83954,\"Z\":24.909912}',-133.228,1,2,12,100),(39,'Garage ACLS','{\"X\":432.75165,\"Y\":-572.5846,\"Z\":27.521606}',144.567,1,2,13,100),(40,'Garage Spedition','{\"X\":1226.044,\"Y\":-3324.5144,\"Z\":4.5216064}',0,1,2,17,100),(41,'Garage Hookies','{\"X\":-2173.7144,\"Y\":4282.1274,\"Z\":48.11206}',-113.386,1,2,25,100),(42,'Garage Skiver','{\"X\":184.6022,\"Y\":2782.6682,\"Z\":45.12964}',14.1732,1,2,22,100),(45,'Hill Valley Church Garage','{\"X\":-1665.2968,\"Y\":-281.38022,\"Z\":50.841797}',-130.394,1,2,34,20),(46,'Garage Spedition Containerhafen','{\"X\":-129.96924,\"Y\":-2536.8,\"Z\":5.0944824}',-119.055,1,2,17,15),(47,'Style & Performance Society Garage','{\"X\":-1412.5055,\"Y\":-457.54285,\"Z\":33.469604}',-121.89,1,2,35,100),(48,'Heligarage Sheriff Department','{\"X\":-450.3956,\"Y\":5984.8613,\"Z\":30.369263}',90.7087,16,2,1,5);
/*!40000 ALTER TABLE `configgarages` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:08:23
