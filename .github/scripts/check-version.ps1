$ErrorActionPreference = 'Stop'

function ConvertTo-SemVer([string]$text) {
	$m = [regex]::Match($text.Trim(), '^v?(?<core>\d+(\.\d+){0,3})(-(?<pre>[0-9A-Za-z.-]+))?(\+.*)?$')
	if (-not $m.Success) { return $null }
	$nums = @($m.Groups['core'].Value.Split('.') | ForEach-Object { [int]$_ })
	while ($nums.Count -lt 4) { $nums += 0 }
	[pscustomobject]@{
		Text = $text.Trim()
		Nums = $nums
		Pre  = if ($m.Groups['pre'].Success) { $m.Groups['pre'].Value.Split('.') } else { @() }
	}
}

function Compare-SemVer($a, $b) {
	for ($i = 0; $i -lt 4; $i++) {
		if ($a.Nums[$i] -ne $b.Nums[$i]) { return [Math]::Sign($a.Nums[$i] - $b.Nums[$i]) }
	}
	if ($a.Pre.Count -eq 0 -and $b.Pre.Count -eq 0) { return 0 }
	if ($a.Pre.Count -eq 0) { return 1 }
	if ($b.Pre.Count -eq 0) { return -1 }
	$len = [Math]::Max($a.Pre.Count, $b.Pre.Count)
	for ($i = 0; $i -lt $len; $i++) {
		if ($i -ge $a.Pre.Count) { return -1 }
		if ($i -ge $b.Pre.Count) { return 1 }
		$x = $a.Pre[$i]; $y = $b.Pre[$i]
		$xn = $x -match '^\d+$'; $yn = $y -match '^\d+$'
		if ($xn -and $yn) {
			if ([long]$x -ne [long]$y) { return [Math]::Sign([long]$x - [long]$y) }
		}
		elseif ($xn) { return -1 }
		elseif ($yn) { return 1 }
		else {
			$c = [string]::CompareOrdinal($x, $y)
			if ($c -ne 0) { return [Math]::Sign($c) }
		}
	}
	return 0
}

$latest = $null
foreach ($tag in (git tag --list)) {
	$v = ConvertTo-SemVer $tag
	if ($null -eq $v) { continue }
	if ($null -eq $latest -or (Compare-SemVer $v $latest) -gt 0) { $latest = $v }
}

if ($null -eq $latest) {
	Write-Host 'No version tags found; nothing to compare against.'
	exit 0
}
Write-Host "Latest tag: $($latest.Text)"

$failed = $false
$projects = Get-ChildItem -Recurse -Filter *.csproj | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
foreach ($proj in $projects) {
	$node = Select-Xml -Path $proj.FullName -XPath '//Version' | Select-Object -First 1
	if ($null -eq $node) { continue }
	$text = $node.Node.InnerText
	$v = ConvertTo-SemVer $text
	if ($null -eq $v) {
		Write-Host "::error file=$($proj.FullName)::Cannot parse version '$text'"
		$failed = $true
		continue
	}
	if ((Compare-SemVer $v $latest) -le 0) {
		Write-Host "::error file=$($proj.FullName)::Version $text must be strictly greater than latest tag $($latest.Text)"
		$failed = $true
	}
	else {
		Write-Host "OK: $($proj.Name) $text > $($latest.Text)"
	}
}

if ($failed) { exit 1 }
