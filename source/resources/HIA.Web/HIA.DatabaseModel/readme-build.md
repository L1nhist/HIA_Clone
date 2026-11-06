###STEP generate DB scafford

1/ Set HIA.DatabaseModel as start project

2/ open package manager console

3/ select default project is HIA.DatabaseModel

4/ change name Entites -> Entites_1

5/ run command as bellow:

Scaffold-DbContext "data source=localhost;initial catalog=HIA_DB;user id=sa;password=Aa,123456;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entites -Context "HIADbContext" -f -UseDatabaseNames -Project HIA.DatabaseModel

6/ remove Entites_1 folder

7/ go to HIADbContext, remove func: protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

8/ generate script overwrite db-script.sql