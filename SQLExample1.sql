/*
A query I made for a report requested for a customer. The query
includes Crystal Reports parameters ({?}) in it. I use a concept I've
read online called transitive closure, basically meaning redundant
joins, to attempt positively influence the query optimizer. I also created a custom field
here called 'Indicator' which basically takes an existing table column value
and puts it into a 'band'. For example, range -100 to 100 would be a band, range
-300 to 300 would be a band, etc. The ranges are dependent upon values provided by the user.
I took advantage of the fact that case statements are evaluated in order to accomplish this.
*/


select
qry.display_employee,
qry.display_employee,
qry.manager_indicator,
qry.payroll_emp_id,
qry.last_name,
qry.first_name,
qry.other_string23 bufu_id,
qry.other_string26 location_code,
qry.other_string19 TEST_code,
qry.c_employee_cost_center,
qry.wfs_string3 manager_name,
qry.other_string10 manager_id,
qry.wfs_string4 hr_responsible,
qry.other_string9 payroll_range,
qry.date_picked,
qry.bank,
qry.balance,
qry.indicator
from
(
select
e.display_employee,
e.manager_indicator,
a.payroll_emp_id,
e.last_name,
e.first_name,
e.other_string23 bufu_id,
e.other_string26 location_code,
e.other_string19 test_code,
e.c_employee_cost_center,
e.wfs_string3 manager_name,
e.other_string10 manager_id,
e.wfs_string4 hr_responsible,
e.other_string9 payroll_range,
{?STD_AS_OF_DATE_SQL} date_picked,
bos.bank,
bos.balance,
case 
when bos.balance between -1 * to_number({?TEST_GREEN_VAL_BAND_SQL}) and  to_number({?TEST_GREEN_VAL_BAND_SQL}) then 'G'
when bos.balance between -1 * (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL})) and  (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL})) then 'Y'
when bos.balance between -1 * (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL}) + to_number({?TEST_RED_VAL_BAND_SQL})) and (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL}) + to_number({?TEST_RED_VAL_BAND_SQL})) then 'R'
else null end indicator
from
employee e,
asgnmt a,
asgnmt_master am,
employee_periods ep,
bank_output_summary bos
where e.employee = am.employee
and am.employee = ep.employee
and e.employee = ep.employee
and a.asgnmt = am.asgnmt
and am.asgnmt = ep.asgnmt
and a.asgnmt = ep.asgnmt
and ep.calc_emp_period_version = bos.employee_period_version
and bos.work_dt between e.eff_dt and e.end_eff_dt
and bos.work_dt between a.eff_dt and a.end_eff_dt
and {?STD_AS_OF_DATE_SQL} between ep.pp_begin and ep.pp_end
and am.asgnmt_type in(1,3)
and a.assignment_status = 'A'
and a.asgnmt in (	
                   select 
                   asgnmt
                   from 
                   asgnmt_grp_detail
                   where asgnmt_grp in ({?STD_ASSIGNMENT_GROUP_LIST_SQL})
                   and {?CURRENT_TIME_SQL} between eff_dttm and end_eff_dttm
           	    )   
and bos.work_dt = (
                    select
                    max(work_dt)
                    from 
                    bank_output_summary
                    where employee_period_version = bos.employee_period_version
                    and bank = bos.bank
                    and work_dt <= {?STD_AS_OF_DATE_SQL}
                  )                                                                                   
and bos.bank in ('BANK1')   
and (                 
		bos.balance between -1 * to_number({?TEST_GREEN_VAL_BAND_SQL}) and  to_number({?TEST_GREEN_VAL_BAND_SQL})
		or bos.balance between -1 * (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL})) and (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL}))
	    or bos.balance between -1 * (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL}) + to_number({?TEST_RED_VAL_BAND_SQL})) and (to_number({?TEST_GREEN_VAL_BAND_SQL}) + to_number({?TEST_YELLOW_VAL_BAND_SQL}) + to_number({?TEST_RED_VAL_BAND_SQL}))
	)
and (
		{?TEST_MANAGER_ID_SQL} is null 
		or {?TEST_MANAGER_ID_SQL} = '' 
		or upper(e.other_string10) like upper({?TEST_MANAGER_ID_SQL})
	)	
) qry
where (qry.indicator = 'G' and {?TEST_GREEN_VAL_EXCLUDE_SQL} = 0)
or (qry.indicator = 'Y' and {?TEST_YELLOW_VAL_EXCLUDE_SQL} = 0)
or (qry.indicator = 'R' and {?TEST_RED_VAL_EXCLUDE_SQL} = 0)
	
