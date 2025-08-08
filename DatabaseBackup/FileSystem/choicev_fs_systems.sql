-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: game.choicev.net    Database: choicev_fs
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
-- Table structure for table `systems`
--

DROP TABLE IF EXISTS `systems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `systems` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `shortName` varchar(45) NOT NULL,
  `abbreviation` varchar(5) NOT NULL,
  `logo` varchar(45) NOT NULL,
  `companyId` int NOT NULL DEFAULT '-1',
  `color` varchar(45) NOT NULL,
  `companyType` int NOT NULL DEFAULT '-1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `systems`
--

LOCK TABLES `systems` WRITE;
/*!40000 ALTER TABLE `systems` DISABLE KEYS */;
INSERT INTO `systems` VALUES (1,'<b>S</b>upport<b>C</b>ontrol<b>P</b>anel','Support System','SCP','support',-1,'#CC8924',0),(2,'<b>C</b>ounty <b>S</b>heriff <b>A</b>dministration <b>S</b>ystem','CoSAS','LSSD','lssd',1,'#45594b',9),(3,'<b>P</b>olice <b>A</b>dministration <b>S</b>ystem','PoLAS','LSPD','lspd',2,'#1e2044',9),(4,'<b>F</b>ire <b>D</b>epartment <b>A</b>dministration <b>S</b>ystem','FireDepAS','FD','lsfd',3,'#ae0925',3),(5,'<b>M</b>edical <b>A</b>dministration <b>S</b>ystem','MedicAS','LSMD','lsmd',4,'#de0000',2),(6,'<b>J</b>ustice <b>A</b>dministration <b>S</b>ystem','JAS','DOJ','doj',5,'#2f5488',-1),(9,'<b>S</b>ocial <b>I</b>nclusion <b>A</b>dministration <b>S</b>ystem','SIAS','DOSI','dosi',7,'#406748',-1),(10,'<b>D</b>aily <b>G</b>lobe <b>A</b>dministration <b>S</b>ystem','DGAS','DG','dg',10,'#406748',-1),(11,'<b>S</b>tadtverwaltung <b>A</b>dministration <b>S</b>ystem','SVAS','STVW','svls',12,'#5597d0',11),(12,'<b>A</b>utomobilclub <b>A</b>dministration <b>S</b>ystem','ACAS','ACLS','acls',13,'#2f5488',16),(13,'<b>G</b>oodmann <b>A</b>dministration <b>S</b>ystem','GAS','ADG','adg',14,'#733237',2),(14,'<b>S</b>pedition <b>A</b>dministration <b>S</b>ystem','SAS','Sped','lslog',17,'#444444',-1),(15,'<b>D</b>epartment of <b>S</b>anitiaton <b>A</b>dministration <b>S</b>ystem','LSDS','LSDS','dos',18,'#406748',-1),(16,'<b>T</b>raffic & <b>T</b>ransport <b>A</b>dministration <b>S</b>ystem','TTSAS','TSATS','ttsas',19,'#70140b',-1),(17,'<b>C</b>ike <b>B</b>ike <b>A</b>dministration <b>S</b>ystem','CBAS','CBS','cbs',20,'#420d08',15),(18,'<b>M</b>agicmoon <b>A</b>dministration <b>S</b>ystem','MoAs','MGM','mgm',21,'#53356c',-1),(19,'<b>S</b>kiver <b>A</b>dministration <b>S</b>ystem','SkAS','BBS','skiver',22,'#ff7e00',-1),(20,'<b>L</b>s <b>M</b>otors <b>A</b>dministration <b>S</b>ystem','lsm','LSM','',23,'#2f5488',15),(21,'<b>P</b>remium <b>D</b>elux <b>M</b>otorsport<b>A</b>dministration <b>S</b>ystem','PeAs','PDM','pdm',24,'#2f5488',15),(22,'<b>H</b>ookies <b>A</b>dministration <b>S</b>ystem','HokAS','HKS','hks',25,'#03286c',-1),(23,'<b>M</b>ultiKulti <b>A</b>dministration <b>S</b>ystem','MuKuVe','MKTV','',26,'#6f7408',-1),(25,'<b>S</b>an <b>A</b>ndreas<b>I</b>nsurance <b>C</b>ompany <b>S</b>ystem','SAIC','SAIC','saic',28,'#ff9c00',-1),(26,'<b>A</b>nwaltskanzlei <b>V</b>ega & <b>B</b>lair <b>S</b>ystem','Kanzlei','AKVB','akvb',30,'#444444',-1),(28,'<b>K</b>urant <b>B</b>ar <b>S</b>ystems','KRNT','KRNT','',33,'#b5a970',-1),(29,'<b>H</b>ill <b>V</b>alley <b>C</b>ommunity <b>C</b>hurch','HVCC','HVCC','hvcc',34,'#eeaf59',-1),(30,'<b>S</b>tyle & <b>P</b>erformance <b>S</b>ociety','SaPS','SaPS','saps',35,'#444444',16),(31,'<b>M</b>editerraneo <b>I</b>talian <b>C</b>uccina','MIC','MIC','mic',36,'#6f7408',-1),(32,'Mediterraneo','MEDIT','MEDIT','',37,'#6f7408',-1);
/*!40000 ALTER TABLE `systems` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:06:03
