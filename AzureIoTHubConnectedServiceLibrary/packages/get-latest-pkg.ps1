function SplitNameToPkgAndVersion($filename) {
    $regex = '.+\\(.+?)\.([\.|\d]+)\.nupkg';
    $values = $filename | select-string -Pattern $regex | % { $_.Matches } | % { $_.Groups } | % { $_.Value }
    if( $values.Count -eq 3 ) {
        return @($values[1], $values[2])
    }
    Write-Host "Error: Unable to extract package name and version from $filename" -foregroundcolor "red"
}

$packages = @();

ls *.nupkg | % {$_.FullName} | % {
    ($pkg, $version) = SplitNameToPkgAndVersion($_)
    $packages += $pkg
    nuget install $pkg -source https://api.nuget.org/v3/index.json
}

# Get the latest versions of everything

$map = @{}

ls -r *.nupkg | % {$_.FullName} | % {

    ($pkg, $version) = SplitNameToPkgAndVersion($_)

    if($map.ContainsKey($pkg))
    {
        $existing = $map.Get_Item($pkg)
        ($dummy, $existing_ver) = SplitNameToPkgAndVersion($existing)
        if($existing_ver -lt $version)
        {
            # Write-Host "Updating $pkg version from $existing_ver to $version"
            $map.Set_Item($pkg, $_)
        }
    }
    else {
        # Write-Host "Adding $pkg version $version"
        $map.Add($pkg, $_)
    }
}

$target_folder = "new"

if(!(Test-Path $target_folder))
{
    mkdir $target_folder
}

$map.Values | % {
    cp $_ $target_folder
}
