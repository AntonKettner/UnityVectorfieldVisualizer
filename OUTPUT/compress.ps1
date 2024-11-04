Write-Host "cutting mp4s together ps1 script started";

Get-ChildItem -Filter video*.mp4 | ForEach-Object { "file '$($_.FullName)'" } | Set-Content -Path mylist.txt;

Get-Content mylist.txt | Out-File -Encoding ASCII -FilePath mylist_temp.txt;

Rename-Item mylist_temp.txt mylist.txt;

# save the no of files named output.mp4 in a variable
$outputno = Get-ChildItem -Filter output*.mp4 | Measure-Object | %{$_.Count};

# compress the video and speed up by factor 1/0.3
ffmpeg -f concat -safe 0 -i mylist.txt -filter:v "setpts=0.5*PTS" -an "output_${outputno}.mp4";

Write-Host "finished";