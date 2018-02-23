

Create database db_Atom

--Use db_Atom

Create table tblVehicleClass
(
	VClassId int identity(1,1) primary key,
	VehicleType varchar(50)
)

Create table tblLicensePlateNo
(
	Id int identity(1,1) primary key,
	PlateNo varchar(20)
)

Create table tblTollOperators
(
	TollCode int identity(1,1) primary key,
	TollOperator varchar(100)
)

Create table tblTollVehicleInfo
(
	EventId int identity(1,1) primary key,
	TollCode int references tblTollOperators(TollCode),
	TransponderId varchar(15) constraint _autoGenKey UNIQUE,
	VClassId int references tblVehicleClass(VClassId),
	LicensePlateNoId int references tblLicensePlateNo(Id),
	InsertDate Datetime,
	UpdateDate Datetime	 
)

	Create view dbo.vw_TollVehicleInfo

	AS
		select a.EventId,a.TransponderId,b.TollOperator,c.PlateNo,
			   d.VehicleType,a.InsertDate,a.UpdateDate
		From tblTollVehicleInfo a
			 JOIN tblTollOperators b on a.TollCode = b.TollCode	
			 JOIN tblLicensePlateNo c on a.LicensePlateNoId = c.Id
			 JOIN tblVehicleClass d on a.VClassId = d.VClassId

	GO