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
-- Table structure for table `configlocker`
--

DROP TABLE IF EXISTS `configlocker`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configlocker` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `companyId` int DEFAULT NULL,
  `dayPrice` decimal(10,2) NOT NULL,
  `colShapeStr` text NOT NULL,
  `phoneNumber` bigint DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_configlocker_phoneNumber_idx` (`phoneNumber`),
  KEY `fk_configlocker_companyId_idx` (`companyId`),
  CONSTRAINT `fk_configlocker_companyId` FOREIGN KEY (`companyId`) REFERENCES `configcompanies` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_configlocker_phoneNumber` FOREIGN KEY (`phoneNumber`) REFERENCES `phonenumbers` (`number`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=57 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configlocker`
--

LOCK TABLES `configlocker` WRITE;
/*!40000 ALTER TABLE `configlocker` DISABLE KEYS */;
INSERT INTO `configlocker` VALUES (7,'Strawberry Metro',NULL,10.00,'{\"X\":254.33847,\"Y\":-1202.3473,\"Z\":28.279907}#5.6#2.0#0.0#true#false#true',12100829439),(8,'Strawberry Metro 2',NULL,10.00,'{\"X\":254.57912,\"Y\":-1205.4154,\"Z\":28.279907}#5.4#1.0#0.0#true#false#true',12100559070),(9,'Del Perro Pier 2',NULL,15.00,'{\"X\":-1617.3132,\"Y\":-1034.1682,\"Z\":12.137695}#7.6#0.9#50.399994#true#false#true',12100876813),(10,'Del Perro Pier',NULL,15.00,'{\"X\":-1618.5759,\"Y\":-1033.9758,\"Z\":12.137695}#9.3#1.3#49.700012#true#false#true',12100175736),(11,'Del Perro Pier 3',NULL,15.00,'{\"X\":-1615.9406,\"Y\":-1035.3934,\"Z\":12.137695}#7.3#0.9#50.0#true#false#true',12100179604),(12,'Tankstelle: Vinewood Mitte',NULL,10.00,'{\"X\":659.03296,\"Y\":270.94943,\"Z\":101.89673}#1.6#3.2#60.399994#true#false#true',12100433305),(13,'Tankstelle: Lago Zancudo 2',NULL,10.00,'{\"X\":-2565.746,\"Y\":2305.2725,\"Z\":32.20581}#0.8#2.1#5.799988#true#false#true',12100905094),(14,'Tankstelle: Lago Zancudo',NULL,10.00,'{\"X\":-2566.3914,\"Y\":2309.646,\"Z\":32.20581}#1.5#3.0#4.899994#true#false#true',12100862772),(15,'Willie\'s: Paleto Bay',NULL,12.50,'{\"X\":-86.479126,\"Y\":6520.008,\"Z\":30.487183}#1.3#11.8#135.0#true#false#true',12100935644),(17,'LSSD Spind 2',1,0.00,'{\"X\":-449.65363,\"Y\":6022.194,\"Z\":30.706177}#2.4776185#1.5446328#44.534943#true#false#true',12100624631),(18,'LSSD Spind 1',1,0.00,'{\"X\":-451.9358,\"Y\":6019.3364,\"Z\":30.706177}#2.4608595#1.7638406#45.630493#true#false#true',12100639278),(19,'LSSD Spind 3',1,0.00,'{\"X\":-448.1011,\"Y\":6020.409,\"Z\":30.706177}#2.577242#1.7696557#44.0744#true#false#true',12100807249),(20,'Spind',NULL,0.00,'{\"X\":448.7209,\"Y\":-984.8571,\"Z\":30.678345}#3.2406414#2.8346355#0.0#true#false#true',12100478854),(21,'Dr.Goodman',14,0.00,'{\"X\":-254.348,\"Y\":6316.8667,\"Z\":32.41394}#1.6269189#3.1622376#43.848694#true#false#true',12100995574),(23,'Traffic Center Spind 1',19,0.00,'{\"X\":-241.32663,\"Y\":-928.4967,\"Z\":32.312866}#3.0#1.6434357#70.1351#true#false#true',12100655841),(24,'Traffic Center Spind 2',19,0.00,'{\"X\":-240.27345,\"Y\":-925.41376,\"Z\":32.312866}#3.5939941#1.7519532#69.4361#true#false#true',12100105732),(26,'Spind',22,0.00,'{\"X\":171.23865,\"Y\":2785.987,\"Z\":46.180176}#4.7583084#0.78948#9.669586#true#false#true',12100394101),(27,'LSPD Spind 1',2,0.00,'{\"X\":844.7646,\"Y\":-1285.1696,\"Z\":28.690063}#2.0226238#5.406006#0.0#true#false#true',12100730335),(28,'LSPD Spind 2',2,0.00,'{\"X\":845.0616,\"Y\":-1280.0411,\"Z\":28.235107}#2.2932944#4.7293296#0.0#true#false#true',12100306950),(31,'LSPD Spind 3',2,0.00,'{\"X\":847.3582,\"Y\":-1280.0016,\"Z\":28.235107}#2.157959#4.593994#0.0#true#false#true',12100539459),(32,'MD_SPND_01',4,0.00,'{\"X\":-336.41284,\"Y\":-230.92747,\"Z\":37.266724}#1.6120753#7.3767543#53.17505#true#false#true',12100847584),(33,'MD_SPND_02',4,0.00,'{\"X\":-334.7196,\"Y\":-228.61986,\"Z\":37.266724}#1.6331402#7.078872#54.41458#true#false#true',12100504711),(34,'MD_SPND_03',4,0.00,'{\"X\":-333.1361,\"Y\":-226.40895,\"Z\":37.266724}#1.6483997#7.30178#54.03241#true#false#true',12100474749),(35,'MD_SPND_04',4,0.00,'{\"X\":-331.3402,\"Y\":-223.985,\"Z\":37.266724}#1.286321#7.3024807#53.647614#true#false#true',12100616783),(38,'Airport',NULL,0.00,'{\"X\":-1022.8136,\"Y\":-2764.7263,\"Z\":21.360474}#0.9399415#4.593994#59.95352#true#false#true',12100274347),(39,'LSFD_EAST_SPIND_1',3,0.00,'{\"X\":1171.5789,\"Y\":-1467.1052,\"Z\":35.07617}#0.9399415#4.1879883#0.0#true#false#true',12100298857),(40,'LSFD_EAST_SPIND_2',3,0.00,'{\"X\":1174.147,\"Y\":-1468.9187,\"Z\":35.07617}#3.7819824#1.3459474#0.0#true#false#true',12100947248),(44,'LSFD_PALETO_SPIND',3,0.00,'{\"X\":-364.6876,\"Y\":6104.9966,\"Z\":31.436646}#3.9173176#2.9549882#45.4599#true#false#true',12100184732),(45,'ACLS Spind',13,0.00,'{\"X\":443.05054,\"Y\":-658.36273,\"Z\":28.58899}#3.4983537#3.7391303#175.34222#true#false#true',12100864850),(46,'Spind Spedition',17,0.00,'{\"X\":1185.4514,\"Y\":-3283.2097,\"Z\":5.5216064}#1.8872885#5.2706704#0.0#true#false#true',12100534105),(47,'Spind 01 LSDOS',18,0.00,'{\"X\":-601.0959,\"Y\":-1618.9714,\"Z\":33.003662}#4.1879883#1.210612#175.14478#true#false#true',12100275529),(48,'Spind 02 LSDOS',18,0.00,'{\"X\":-599.33813,\"Y\":-1616.822,\"Z\":33.003662}#5.082972#0.90634626#173.46484#true#false#true',12100579859),(49,'Spind 03 LSDOS',18,0.00,'{\"X\":-592.41895,\"Y\":-1617.3231,\"Z\":33.003662}#3.5284822#0.95332617#173.85333#true#false#true',12100649241),(50,'Hookies Spind 01',25,0.00,'{\"X\":-2178.127,\"Y\":4285.573,\"Z\":49.162598}#0.86844254#4.596207#59.881195#true#false#true',12100950877),(51,'Hookies Spind 02',25,0.00,'{\"X\":-2179.0945,\"Y\":4283.565,\"Z\":49.162598}#0.8813276#4.5567875#60.722565#true#false#true',12100772849),(52,'ECC Spind',1,0.00,'{\"X\":300.5899,\"Y\":-299.88483,\"Z\":60.14868}#1.133674#3.1928346#70.22525#true#false#true',12100638391),(53,'ECC Spind',2,0.00,'{\"X\":298.27252,\"Y\":-305.8945,\"Z\":60.14868}#0.727017#3.1606028#71.23273#true#false#true',12100395859),(54,'ECC Spind',3,0.00,'{\"X\":298.17975,\"Y\":-302.42636,\"Z\":60.14868}#5.8120117#1.612727#71.217316#true#false#true',12100125513),(55,'Style & Performance Society Spind',35,0.00,'{\"X\":-1380.0939,\"Y\":-459.83737,\"Z\":34.654907}#0.6644538#0.6278226#0.0#true#false#true',12100676771),(56,'Mediterraneo ',36,0.00,'{\"X\":428.5192,\"Y\":-1505.9341,\"Z\":29.313599}#1.6079433#0.783778#29.266968#true#false#true',12100163992);
/*!40000 ALTER TABLE `configlocker` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:10:18
