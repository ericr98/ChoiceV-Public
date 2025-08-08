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
-- Table structure for table `configminijobs`
--

DROP TABLE IF EXISTS `configminijobs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configminijobs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `type` int NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` text NOT NULL,
  `requirements` varchar(255) NOT NULL,
  `maxUses` int NOT NULL DEFAULT '-1',
  `information` varchar(255) NOT NULL,
  `workTimeHour` float NOT NULL DEFAULT '1.5',
  `rewardJson` text,
  `blockMultiUse` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=43 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configminijobs`
--

LOCK TABLES `configminijobs` WRITE;
/*!40000 ALTER TABLE `configminijobs` DISABLE KEYS */;
INSERT INTO `configminijobs` VALUES (2,0,'Unsere Parks sollen schöner werden Part I','Wir brauchen jemanden, der sich um den großen Brunnen neben dem Filmstudio kümmert und die Umgebung etwas aufräumt.','Wasserfestes Schuhwerk oder kein Problem mit nassen Füßen',-1,'Dauer: 20min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(3,2,'Unsere Parks sollen schöner werden Part II','Es wird jemand benötigt, der unsere Parks ein bisschen aufräumt.','Fahrzeug wird empfohlen',-1,'Dauer: 25min',1.5,'[{\"CashReward\":200.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(7,0,'Mehr Energie!','Elektriker Walden benötigt Hilfe. ','-',-1,'Dauer: 45min',1.5,'[{\"CashReward\":280.50,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(9,0,'Aufgabenliste für Umweltbewusste!','Wir brauchen tatkräftige Unterstützung bei folgenden Aufgaben.....','Fahrzeug wird empfohlen',-1,'Dauer: 30min',1.5,'[{\"CashReward\":133.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(10,0,'Säuberungsaktion von Paleto Bay!','Es scheint, als ob die Bürger von Paleto Bay den Wunsch nach einer saubereren Stadt haben, aber niemand ist bereit, aktiv dazu beizutragen. In Anbetracht dieser Situation wird Unterstützung von außerhalb gesucht, und es wird eine Gegenleistung angeboten.','Sauberes Erscheinungsbild, Belastbar und Schmerzbefreit',-1,'Dauer: 35min',1.5,'[{\"CashReward\":150.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(11,0,'Krieg der Generationen','Infos stehen auf der Aufgabenliste am Strand.','Motiviert, Neutral und Geruchsunempfindlich',-1,'Dauer: 20min',1.5,'[{\"CashReward\":140.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(12,0,'Bauleiter abhandengekommen!','Bauleiter Kalle braucht Hilfe bei der Baustelle.','keine Höhenangst, Belastbar, Schwindelfrei, gut in Tanken (Bier)',-1,'Dauer: 30min',1.5,'[{\"CashReward\":200.50,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(14,0,'Die verwirrte Oma Gertrude braucht Hilfe!','Oma Gertrude ist mal wieder verwirrt und braucht Hilfe.','Sehr viel Geduld',-1,'Dauer: 25min',1.5,'[{\"CashReward\":152.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(15,0,'Reinigungsarbeiten Sandy Shores ','Fahre zum Schwarzen Brett nach Sandy Shores um Aufgaben zu bekommen.','Auto wünschenswert',-1,'Dauer: 20min',1.5,'[{\"CashReward\":110.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(16,0,'Heikle Staatsangelegenheit!','Es scheint, als ob Mr. Jackson Jones dringend Hilfe bei der Organisation einer wichtigen Angelegenheit benötigt und alleine damit überfordert ist. Es wäre ratsam, sich mit Mr. Jones zu treffen und ihm deine Unterstützung anzubieten.','Verschwiegenheit',-1,'Dauer: 20min',1.5,'[{\"CashReward\":152.50,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(17,0,'Eskalation Pacific Bluffs Country Club','Der Pacific Bluffs Country Club liegt, wie der Name bereits verrät, in Pacific Bluffs am Stadtrand von Los Santos am Great Ocean Highway. Der Golfclub bietet eine Vielzahl von Freizeitaktivitäten wie Jetski, Surfen, Tennis, Beachvolleyball und Korbball. Auf seinem Parkplatz finden 63 Fahrzeuge Platz, wobei vier Stellplätze für Behinderte reserviert sind. Der Club veranstaltet zahlreiche Events, bei denen es lebhaft zugeht. Gelegentlich benötigen sie Unterstützung und haben eine Liste mit Aufgaben, die erledigt werden müssen. Es könnte sich lohnen, sich dort nach Möglichkeiten zur Hilfe umzusehen.','Fahrzeug wird empfohlen',-1,'Dauer: 20min',1.5,'[{\"CashReward\":309.50,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(19,0,'Hausmeister in Ruhestand','Sehr geehrte Damen und Herren, unser Hausmeister ist in den Ruhestand gegangen, und da wir die Stelle nicht mehr besetzen werden, suchen wir nach Aushilfskräften, die rund um die Hall von Davis verschiedene Aufgaben erledigen können. Dort hängt eine Liste mit Aufgaben aus, die abgearbeitet werden müsste.','-',-1,'Dauer: 20min',1.5,'[{\"CashReward\":172.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(20,0,'Mexikanisches Familienfest','Sehr geehrte Damen und Herren, der Staat von San Andreas legt großen Wert darauf, dass Menschen, die nicht in Amerika geboren wurden, gut integriert werden. Die Förderung von kulturellen Feiern wird aktiv vom Staat unterstützt. Das benötigte Equipment wird vom Staat bereitgestellt, jedoch gibt es oft Schwierigkeiten bei der Rückgabe. Aus diesem Grund suchen wir jemanden, der sich um dieses Anliegen kümmert. Im Mexikanischen Viertel hängt eine Liste mit Aufgaben aus. Wir würden uns freuen, wenn Sie Interesse an dieser Aufgabe haben und zur erfolgreichen Umsetzung beitragen könnten.','-',-1,'Dauer: 15min',1.5,'[{\"CashReward\":100.0,\"Items\":[279],\"ItemsAmount\":[5],\"RankName\":null,\"rankExp\":0}]',1),(22,0,'Arbeiten beim Maze Bank Tower ','Die Maze Bank von Los Santos sucht gelegentlich fleißige Mitarbeiter, die das Gelände rund um ihren Hauptsitz sauber und gepflegt halten. Wenn du nähere Informationen erhalten möchtest, begebe dich bitte dorthin und suche nach einer Liste mit Aufgaben.','-',-1,'Dauer: 15min',1,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(23,0,'Aushilfe in der Fußgängerzone gesucht!','Mrs. Cooper benötigt eine Aushilfskraft, die sich um die Fußgängerzone kümmert. Zu den Aufgaben gehören das Aufsammeln von Müll, die Überprüfung von Sitzmöglichkeiten sowie die Pflege der Pflanzen. Falls Sie Interesse an dieser Aufgabe haben, wenden Sie sich bitte direkt an Mrs. Cooper oder schauen Sie vor Ort nach weiteren Informationen.','-',-1,'Dauer: 15min',1.5,'[{\"CashReward\":200.50,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(24,0,'Ansprüche der High Society','Begebe dich zum Filmset an die Pforte und spreche mit Mr. Ling','Gute nerven ',-1,'Dauer: 15min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(25,0,'Yachtclub','Lizzy Jones ist einer der Manager des “LOS SANTOS MARINA YACHT CLUB” und benötigt Unterstützung. Begib dich bitte dorthin und biete ihr deine Hilfe an.','-',-1,'Dauer: 20min',1.5,'[{\"CashReward\":300.50,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(26,0,'Der verrückte Professor','Der Professor sucht jemanden, der ihn bei seinen Experimenten unterstützt, er kann gerade nicht weg, braucht aber Sachen.','starke Nerven',-1,'Dauer: 20min',1.5,'[{\"CashReward\":153.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(27,0,'Verzweifelte Mary','Beim Fischerdorf braucht Mary Unterstützung','Viel Geld im Vorraus',-1,'Dauer: 25min',1.5,'[{\"CashReward\":230.45,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(28,0,'Eltern im Urlaub','Tracy De Santa benötigt dringend Hilfe im Haus. Fahre zu ihr und biete deine Unterstützung an.','',-1,'Dauer: 15min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(29,0,'Instandsetzung der Metro I','Ein Mitarbeiter der Metro benötigt Unterstützung. Fahre zur Metro und hilf ihm.','',-1,'Dauer: 20min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(30,0,'Instandsetzung der Metro II','Ein Mitarbeiter der Metro benötigt Unterstützung. Fahre zur Metro und hilf ihm.','',-1,'Dauer: 20min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(32,0,'Der gescheiterte Sheriff','Sheriff Dave Smith benötigt Hilfe!','',-1,'Dauer: 15min',1.5,'[{\"CashReward\":177.0,\"Items\":[238],\"ItemsAmount\":[1],\"RankName\":null,\"rankExp\":0}]',1),(33,0,'Bürgermeister kommt zu Besuch.','B.J. Smith Recreation Center sucht dringend Unterstützung.','',-1,'Dauer: 20min',1.5,'[{\"CashReward\":200.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(34,0,'Instandsetzung Metro III','Ein Mitarbeiter bei der Metrostation benötigt Hilfe.','',-1,'Dauer: 10min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(35,0,'Brief - Kurier','Global Postal braucht deine Unterstützung. Viele Kollegen sind krank geworden. Bitte übernimm die Auslieferung unserer Briefe.','Fahrrad oder anders Bewegungsmittel.',-1,'Dauer: 10min',1.5,'[{\"CashReward\":100.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(36,0,'Das Auto des hippen Priesters ist kaputt.','Pastor Frank braucht an der Kirche nahe der Route 68 Hilfe.','Ein Fahrzeug',-1,'Dauer: 10min',1.5,'[{\"CashReward\":375.75,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(37,1,'Knastarbeiten I','Verichte arbeiten um dein Taschengeld zu erhöhen.','',-1,'Dauer: 5min',1,'[{\"CashReward\":20.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(38,1,'Knastarbeiten II','Verichte arbeiten um dein Taschengeld zu erhöhen.','',-1,'Dauer: 5min',1,'[{\"CashReward\":20.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(39,1,'Knastarbeiten III','Verichte arbeiten um dein Taschengeld zu erhöhen.','',-1,'Dauer: 5min',1,'[{\"CashReward\":20.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1),(40,1,'Knastarbeiten IIII','Verichte arbeiten um dein Taschengeld zu erhöhen.','',-1,'Dauer: 5min',1,'[{\"CashReward\":20.0,\"Items\":[],\"ItemsAmount\":[],\"RankName\":null,\"rankExp\":0}]',1);
/*!40000 ALTER TABLE `configminijobs` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-02-23 20:11:32
