<?php
require "database.php";

error_reporting(E_ALL);
ini_set('display_errors', 1);

function toCommunityID($id) {
    if (preg_match('/^STEAM_/', $id)) {
        $parts = explode(':', $id);
        return bcadd(bcadd(bcmul($parts[2], '2'), '76561197960265728'), $parts[1]);
    }
    return is_numeric($id) && strlen($id) < 16 ? bcadd($id, '76561197960265728') : $id;
}

$isDarkMode = !isset($_COOKIE['darkMode']) || (isset($_COOKIE['darkMode']) && $_COOKIE['darkMode'] === 'true');

$perPageOptions = [10, 25, 50];
$perPage = isset($_GET['perPage']) ? (int)$_GET['perPage'] : 10;
if (!in_array($perPage, $perPageOptions)) {
    $perPage = 10;
}
$page = max(1, (int)($_GET['page'] ?? 1));
$offset = ($page - 1) * $perPage;

$search = trim($_GET['search'] ?? $_POST['search'] ?? '');
$whereClause = '';
$params = [];

if ($search !== '') {
    $whereClause = "WHERE CONCAT(date, steam_id, player_name, message) LIKE :search";
    $params[':search'] = "%$search%";
}

$query = "SELECT SQL_CALC_FOUND_ROWS * FROM chat_logs $whereClause ORDER BY id DESC LIMIT :limit OFFSET :offset";
$stmt = $pdo->prepare($query);

foreach ($params as $key => $value) {
    $stmt->bindValue($key, $value);
}
$stmt->bindValue(':limit', $perPage, PDO::PARAM_INT);
$stmt->bindValue(':offset', $offset, PDO::PARAM_INT);
$stmt->execute();
$messages = $stmt->fetchAll();

$totalMessages = $pdo->query("SELECT FOUND_ROWS()")->fetchColumn();
$totalPages = (int)ceil($totalMessages / $perPage);

$noResultsMessage = '';
if (empty($messages)) {
    if ($search !== '') {
        $noResultsMessage = "No results found for <strong>" . htmlspecialchars($search) . "</strong>";
    } else {
        $noResultsMessage = "No messages have been logged yet.";
    }
}

function getTeamClass(int $where): string {
    return match($where) {
        4, 5 => 't',
        6, 7 => 'ct',
        default => 'spec'
    };
}

function getTeamPrefix(int $where): string {
    return match($where) {
        1 => '',
        3 => '[TEAM] ',
        5 => '[TEAM] ',
        7 => '[TEAM] ',
        default => ''
    };
}
?>


<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>[CS2] Chat Logger</title>
    <link rel="stylesheet" href="style.css">
	<link rel="icon" href="icon_small.png" type="image/png">
</head>
<body class="<?= $isDarkMode ? 'dark' : 'light' ?>">
    <header class="header-container">
        <h1 class="logo-inside-title">
            <img src="icon_site.png" 
                 alt="CS2" class="inline-logo">
            Chat Logger
        </h1>
        <div class="theme-toggle">
			<label class="switch">
				<input type="checkbox" id="themeCheckbox" onchange="toggleTheme()" <?= $isDarkMode ? 'checked' : '' ?>>
				<span class="slider-container">
					<!-- Visible track background -->
					<span class="slider-track-bg"></span>
					
					<!-- Animated path -->
					<span class="slider-path"></span>
					
					<!-- Slider ball with icons -->
					<span class="slider-ball">
						<span class="sun">
							<svg viewBox="0 0 24 24">
								<circle cx="12" cy="12" r="5" fill="#FFD700"/>
								<path d="M12 5v2M12 17v2M5 12h2M17 12h2" stroke="#FFD700" stroke-width="2" stroke-linecap="round"/>
								<path d="M7.05 7.05l1.41 1.41M15.54 15.54l1.41 1.41" stroke="#FFD700" stroke-width="2"/>
								<path d="M7.05 16.95l1.41-1.41M15.54 8.46l1.41-1.41" stroke="#FFD700" stroke-width="2"/>
							</svg>
						</span>
						<span class="moon">
							<svg viewBox="0 0 24 24">
								<path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" fill="#000000"/>
							</svg>
						</span>
					</span>
				</span>
			</label>
		</div>
    </header>

    <main>
        <form method="get" class="searchBox">
			<input class="searchInput" name="search" type="text" 
				   placeholder="Search... Time/SteamID/Name/Message" value="<?= htmlspecialchars($search) ?>">
			<button class="searchButton" type="submit">
				<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24">
					<path d="M23.809 21.646l-6.205-6.205c1.167-1.605 1.857-3.579 1.857-5.711 0-5.365-4.365-9.73-9.731-9.73-5.365 0-9.73 4.365-9.73 9.73 0 5.366 4.365 9.73 9.73 9.73 2.034 0 3.923-.627 5.487-1.698l6.238 6.238 2.354-2.354zm-20.955-11.916c0-3.792 3.085-6.877 6.877-6.877s6.877 3.085 6.877 6.877-3.085 6.877-6.877 6.877c-3.793 0-6.877-3.085-6.877-6.877z"/>
				</svg>
			</button>
		</form>
		
		<div class="page-controls">
			<form method="get" class="per-page-form">
				<input type="hidden" name="search" value="<?= htmlspecialchars($search) ?>">
				<select name="perPage" onchange="this.form.submit()" class="per-page-select">
					<option value="10" <?= $perPage === 10 ? 'selected' : '' ?>>10 per page</option>
					<option value="25" <?= $perPage === 25 ? 'selected' : '' ?>>25 per page</option>
					<option value="50" <?= $perPage === 50 ? 'selected' : '' ?>>50 per page</option>
				</select>
			</form>
		</div>

        <table class="chat-table">
            <thead>
                <tr>
                    <th>Time</th>
                    <th>SteamID</th>
                    <th>Name</th>
                    <th>Message</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($messages as $message): ?>
                <tr>
                    <td><?= htmlspecialchars($message['date']) ?></td>
                    <td>
                        <a href="https://steamcommunity.com/profiles/<?= toCommunityID($message['steam_id']) ?>" 
                           class="steamid" target="_blank">
                            <?= htmlspecialchars($message['steam_id']) ?>
                        </a>
                    </td>
                    <td><?= htmlspecialchars($message['player_name']) ?></td>
                    <td class="message <?= getTeamClass((int)$message['where']) ?>">
                        <?= getTeamPrefix((int)$message['where']) ?>
                        <?= htmlspecialchars($message['message']) ?>
                    </td>
                </tr>
                <?php endforeach; ?>
				<?php if (!empty($noResultsMessage)): ?>
				<tr class="no-messages">
					<td colspan="4"><?= $noResultsMessage ?></td>
				</tr>
				<?php endif; ?>
            </tbody>
        </table>

        <?php if ($totalPages > 1): ?>
        <div class="pagination">
            <div class="page-info">Page <?= $page ?> of <?= $totalPages ?></div>
            <div class="page-numbers">
                <?php if ($page > 1): ?>
					<a class="letter" href="?page=<?= $page-1 ?>&search=<?= urlencode($search) ?>&perPage=<?= $perPage ?>">Previous</a>
				<?php endif; ?>
                
                <?php 
                $startPage = max(1, $page - 2);
                $endPage = min($totalPages, $page + 2);
                
                if ($startPage > 1): ?>
                    <a class="letter" href="?page=1&search=<?= urlencode($search) ?>">1</a>
                    <?php if ($startPage > 2): ?>
                        <span class="letter">...</span>
                    <?php endif; ?>
                <?php endif; ?>
                
                <?php for ($i = $startPage; $i <= $endPage; $i++): ?>
                    <a class="letter <?= $i === $page ? 'active' : '' ?>" 
						href="?page=<?= $i ?>&search=<?= urlencode($search) ?>&perPage=<?= $perPage ?>"><?= $i ?></a>
                <?php endfor; ?>
                
                <?php if ($endPage < $totalPages): ?>
                    <?php if ($endPage < $totalPages - 1): ?>
                        <span class="letter">...</span>
                    <?php endif; ?>
                    <a class="letter" href="?page=<?= $totalPages ?>&search=<?= urlencode($search) ?>"><?= $totalPages ?></a>
                <?php endif; ?>
                
                <?php if ($page < $totalPages): ?>
                    <a class="letter" href="?page=<?= $page+1 ?>&search=<?= urlencode($search) ?>&perPage=<?= $perPage ?>">Next</a>
                <?php endif; ?>
            </div>
        </div>
        <?php endif; ?>
    </main>

	<footer class="footer">
		<div>Created by <a class="venus" href="https://github.com/oqyh/">Gold KingZ</a></div>
		<a href="https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ" class="version" target="_blank" rel="noopener noreferrer">1.0.0</a>
	</footer>

    <script>
		if (document.cookie.indexOf('darkMode=') === -1) {
			document.cookie = 'darkMode=true; path=/; max-age=31536000; SameSite=Lax';
		}

		function toggleTheme() {
			const isDark = !document.body.classList.contains('dark');
			document.cookie = `darkMode=${isDark}; path=/; max-age=31536000; SameSite=Lax`;
			document.body.classList.toggle('dark', isDark);
			
			const ball = document.querySelector('.slider-ball');
			ball.style.boxShadow = `0 0 15px ${isDark ? 'rgba(224,224,224,0.7)' : 'rgba(253,184,19,0.7)'}, 
								  inset 0 -2px 5px rgba(0,0,0,0.1)`;
			
			setTimeout(() => {
				ball.style.boxShadow = '';
			}, 500);
		}

		document.addEventListener('DOMContentLoaded', function() {
			if (document.cookie.indexOf('darkMode=true') !== -1) {
				document.body.classList.add('dark');
			}
		});
	</script>
</body>
</html>