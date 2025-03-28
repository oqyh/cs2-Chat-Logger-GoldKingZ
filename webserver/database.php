<?php
	$host = 'MySql_Host';
	$db   = 'MySql_Database';
	$user = 'MySql_Username';
	$pass = 'MySql_Password';
	$port = '3306';
	$charset = 'utf8mb4';

	$dsn = "mysql:host=$host;port=$port;dbname=$db;charset=$charset";

	$options =
	[
		PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
		PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
		PDO::ATTR_EMULATE_PREPARES   => false,
	];

	try
	{
		$pdo = new PDO($dsn, $user, $pass, $options);
	}
	catch (PDOException $e)
	{
		die("Database connection failed: " . $e->getMessage());
	}
?>