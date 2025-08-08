#!/usr/bin/env nodejs

const http = require('http');
const httpProxy = require('http-proxy');
const url = require('url');
var config = require('./config.json');

// Create a proxy server
const proxy = httpProxy.createProxyServer({});
const mysql = require('mysql');


// Looks something like:
//http://localhost:81/radio.mp3?target=https://azuracast.choicev-cef.net:8010&userId=577&token=98f45058-a21b-4d12-a37e-b51559588cad&schema=choicev_erk

// Create a HTTP server
http.createServer((req, res) => {
  try {
    var params = url.parse(req.url, true).query;
    const target = params.target;
    const schema = params.schema;

    const userId = params.userId;
    const token = params.token;

    if (typeof userId != "string" || typeof token != "string") {
      return;
    }

    if (userId && token) {
      const connection = mysql.createConnection({
        host: config.host,
        user: config.user,
        password: config.password,
        database: schema
      });

      connection.connect((err) => {
        if (err) {
          console.error(err);   
          connection.end();
        }
      });

      connection.query('SELECT * FROM characters WHERE id = ? LIMIT 1', [userId], (err, result) => {
        if(err) {
          console.error(err);
          connection.end();
        };

        if(result != null && result[0].loginToken == token) {
          // Proxy the request
          proxy.web(req, res, {
            target: target,
            secure: false,
          });
          connection.end();
        } else {
          connection.end();

          res.writeHead(403, {
            'Content-Type': 'text/plain'
          });
          res.end('Forbidden. Connect on the server!');
        }
      });   
    }
  } catch (e) {
    console.error(e);
  }
}).listen(81);

process.on('uncaughtException', function (error) {
  console.log(error.stack);
});

console.log('Proxy server (V3) running on port 81');