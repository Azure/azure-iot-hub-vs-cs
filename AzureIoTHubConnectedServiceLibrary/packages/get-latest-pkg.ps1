function SplitNameToPkgAndVersion($filename) {
    $regex = '.+\\(.+?)\.([\.|\d]+)\.nupkg';
    $values = $filename | select-string -Pattern $regex | % { $_.Matches } | % { $_.Groups } | % { $_.Value }
    if( $values.Count -eq 3 ) {
        return @($values[1], $values[2])
    }
    Write-Host "Error: Unable to extract package name and version from $filename" -foregroundcolor "red"
}

$target_folder = "new"

if(Test-Path $target_folder)
{
    Remove-Item $target_folder -recurse
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
        if( (New-Object System.Version($existing_ver)) -lt (New-Object System.Version($version)))
        {
            Write-Host "Updating $pkg version from $existing_ver to $version" -foregroundcolor "magenta"
            $map.Set_Item($pkg, $_)
        }
    }
    else {
        Write-Host "Adding $pkg version $version"
        $map.Add($pkg, $_)
    }
}

mkdir $target_folder

$map.Values | % {
    cp $_ $target_folder
}

# Iterate over the old nupkg files and see if we have a newer version

ls *.nupkg | % {$_.FullName} | % {
    ($pkg, $version) = SplitNameToPkgAndVersion($_)
    $search_path = "$target_folder\\*.nupkg"
    $up_to_date = $True
    ls $search_path | % {$_.FullName} | % {
        ($pkg2, $version2) = SplitNameToPkgAndVersion($_)
        if($pkg -eq $pkg2)
        {
            if( (New-Object System.Version($version)) -lt (New-Object System.Version($version2)))
            {
                Write-Host "$pkg version $version --> $version2" -foregroundcolor "magenta"
                $up_to_date = $False
            }
        }
    }
    if($up_to_date -eq $True)
    {
        Write-Host "$pkg is up to date" -foregroundcolor "gray"
    }
}
