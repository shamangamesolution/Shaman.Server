file=shaman.env.mm.$1.list
ip=$(dig @resolver1.opendns.com ANY myip.opendns.com +short)
echo PublicDomainNameOrAddress=$ip > "$file"
echo RouterUrl=https://rw_eur.***REMOVED***.com:7002 >> "$file"
echo Metrics__GraphiteUrl=net.tcp://***REMOVED*** >> "$file"
echo Metrics__Path=RW.AWS.MM.$1.$(echo $ip | sed -e "s/\./_/g") >> "$file"
echo COMPlus_PerfMapEnabled=1 >> "$file"
echo BindToPortHttp=$3 >> "$file"
echo Ports=$2 >> "$file"
echo Serilog__customerToken=TODO = >> "$file"
echo Serilog__MinimumLevel=Error  >> "$file"
