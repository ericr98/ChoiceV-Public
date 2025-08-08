<?php
error_reporting(0);

// Load JWT Libary
require __DIR__ . '/vendor/autoload.php';

include('config.php');

use Firebase\JWT\JWT;

$userId = $_GET["userId"];
$schema = $_GET["schema"];
$discordToken = $_GET["discordToken"];
$guid = $_GET["guid"];
$loginToken = $_GET["token"];

$connection = new mysqli(DATABASE_SERVER_SERVER, DATABASE_SERVER_USERNAME, DATABASE_SERVER_PASSWORD);
mysqli_set_charset($connection, "utf8");
$connection->select_db($schema);

$stmt = $connection->prepare("SELECT * FROM characters c
                    WHERE c.id = ? AND loginToken = ? LIMIT 1");

$stmt->bind_param("is", $userId, $loginToken);
if ($stmt->execute()) {
    $dbResult = $stmt->get_result()->fetch_assoc();

    if($dbResult != null) {
        $discordAuthenticated = false;
        $result = apiRequest("https://discordapp.com/api/users/@me", $discordToken);

        $sptStatement = $connection->prepare("SELECT * FROM accounts WHERE id = ? LIMIT 1");
        $sptStatement->bind_param("i", $dbResult["accountId"]);

        if (!OVERRIDE_DISCORD_AUTH && $sptStatement->execute()) {
            $account = $sptStatement->get_result()->fetch_assoc();

            if ($account["discordId"] == $result->id) {     
                $discordAuthenticated = true;
            }
        }

        if($discordAuthenticated || OVERRIDE_DISCORD_AUTH) {
            $userName = "";
            $socialMediaAccount = $connection->prepare("SELECT * FROM socialmediaaccounts WHERE guid = ? LIMIT 1");
            $socialMediaAccount->bind_param("s", $guid);
            if ($socialMediaAccount->execute()) {
                $socialMediaAccountResult = $socialMediaAccount->get_result()->fetch_assoc();
                if($socialMediaAccountResult["ownerType"] == 0) {
                    if($socialMediaAccountResult["ownerId"] == $userId) {
                        $userName = $socialMediaAccountResult["userName"];
                    }
                } else if($socialMediaAccountResult["ownerType"] == 1) {
                    $split = explode("#", $socialMediaAccountResult["currentLoginAttempt"]);

                    if($split[0] == $userId && strtotime($split[1]) > time() - 5) {
                        $userName = $socialMediaAccountResult["userName"];
                    }
                }


                if($userName != "") {
                    $key = SOCIAL_MEDIA_JWT_KEY;
                    $domain = SOCIAL_MEDIA_HOST;
                    $urlRewritingEnabled = true;
        
                    // Build token including your user data
                    $now = time();
                    $token = array(
                        'iss' => 'JWT-Provider',
                        'jti' => md5($now . rand()),
                        'iat' => $now,
                        'exp' => $now + 60,
                        'username' => $userName,    
                        'guid' => $guid,
                        'email' => $userName . "@none.none",
                    );
        
                    // Create JWT token
                    $jwt = JWT::encode($token, $key);
        
                    // Redirect user back to humhub
                    if ($urlRewritingEnabled) {
                        $location = $domain . '/user/auth/external?authclient=jwt';
                    } else {
                        $location = $domain . '/index.php?r=/user/auth/external&authclient=jwt';
                    }
                    $location .= "&jwt=" . $jwt;
        
                    header("Location: " . $location);
                }
            }
        }
    }
}

function apiRequest($url, $token) {
    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE);
  
    $response = curl_exec($ch);
  
    $headers[] = 'Accept: application/json';
    $headers[] = 'Authorization: Bearer ' . $token;
  
    curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
  
    $response = curl_exec($ch);
    return json_decode($response);
  }
?>
