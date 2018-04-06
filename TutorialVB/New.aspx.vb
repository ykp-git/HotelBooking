' Copyright � 2005 - 2013 Annpoint, s.r.o.
'   Use of this software is subject to license terms. 
'   http://www.daypilot.org/
'
'   If you have purchased a DayPilot Pro license, you are allowed to use this 
'   code under the conditions of DayPilot Pro License Agreement:
'
'   http://www.daypilot.org/files/LicenseAgreement.pdf
'
'   Otherwise, you are allowed to use it for evaluation purposes only under 
'   the conditions of DayPilot Pro Trial License Agreement:
'   
'   http://www.daypilot.org/files/LicenseAgreementTrial.pdf
'   
'

Imports System
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.UI

Partial Public Class [New]
	Inherits Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
		If Not IsPostBack Then
			TextBoxStart.Text = Convert.ToDateTime(Request.QueryString("start")).ToShortDateString()
			TextBoxEnd.Text = Convert.ToDateTime(Request.QueryString("end")).ToShortDateString()

			TextBoxName.Focus()

			DropDownList1.DataSource = dbGetResources()
			DropDownList1.DataTextField = "RoomName"
			DropDownList1.DataValueField = "RoomId"
			DropDownList1.SelectedValue = Request.QueryString("r")
			DropDownList1.DataBind()
		End If
	End Sub
	Protected Sub ButtonOK_Click(ByVal sender As Object, ByVal e As EventArgs)
		Dim start As Date = Convert.ToDateTime(TextBoxStart.Text).Date.AddHours(12)
		Dim [end] As Date = Convert.ToDateTime(TextBoxEnd.Text).Date.AddHours(12)
		Dim name As String = TextBoxName.Text
		Dim resource As String = DropDownList1.SelectedValue

		dbInsertEvent(start, [end], name, resource, 0)
		Modal.Close(Me, "OK")
	End Sub

	Private Function dbGetResources() As DataTable
		Dim da As New SqlDataAdapter("SELECT [RoomId], [RoomName] FROM [Room]", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
		Dim dt As New DataTable()
		da.Fill(dt)

		Return dt
	End Function

	Private Sub dbInsertEvent(ByVal start As Date, ByVal [end] As Date, ByVal name As String, ByVal resource As String, ByVal status As Integer)
		Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			con.Open()
			Dim cmd As New SqlCommand("INSERT INTO [Reservation] (ReservationStart, ReservationEnd, ReservationName, RoomId, ReservationStatus) VALUES(@start, @end, @name, @resource, @status)", con)
			'cmd.Parameters.AddWithValue("id", id);
			cmd.Parameters.AddWithValue("start", start)
			cmd.Parameters.AddWithValue("end", [end])
			cmd.Parameters.AddWithValue("name", name)
			cmd.Parameters.AddWithValue("resource", resource)
			cmd.Parameters.AddWithValue("status", status)
			cmd.ExecuteNonQuery()
		End Using
	End Sub

	Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As EventArgs)
		Modal.Close(Me)
	End Sub
End Class
