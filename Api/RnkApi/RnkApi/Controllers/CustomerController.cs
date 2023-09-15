using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RnkApi.Context;
using RnkApi.Migrations;
using RnkApi.Models;
using System;

namespace RnkApi.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly appDbContext _appDbContext;
        public CustomerController(appDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }



        [HttpGet("GetAllcustomer")]
        public async Task<IActionResult> GetAllcustomer()
        {
            try
            {
                var cus = await _appDbContext.Customers.ToListAsync();
                return Ok(cus);
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        [HttpPost("addcustomer")]
        public async Task<IActionResult> addcustomer(Customer oCustomer)
        {
            try
            {

                // var AddEmplo = await appDbcontext.Employees.AddAsync(oEmployees);
                //appDbcontext.SaveChanges();
                //return Ok(AddEmplo);

                oCustomer.id = Guid.NewGuid();
                await _appDbContext.Customers.AddAsync(oCustomer);
                await _appDbContext.SaveChangesAsync();
                return Ok(oCustomer);



            }
            catch (System.Exception)
            {
                return Ok();
                throw;

            }


        }

        //insert

        [HttpPost("SetCustomer")]
        public async Task<IActionResult> SetCustomer([FromBody] Customer userObj)
        {
            if (userObj == null)

                return BadRequest();

            //check username
            if (await checkUserNameExistAsync(userObj.c_FullName))
                return BadRequest(new { Message = "UserName Already Exist" });

            //check mail
            if (await checkAddresExistAsync(userObj.c_Address))
                return BadRequest(new { Message = "Address Already Exist" });

           // var  contact = Convert.ToInt32("C_ContactNo");

            if (await checkContactNumberExistAsync(userObj.c_ContactNo))
                return BadRequest(new { Message = "Contact Already Exist" });


            if (await checkNicExistAsync(userObj.c_Nic))
                return BadRequest(new { Message = "Nic Already Exist" });

           
           
            userObj.modifiedUser = "User";
            userObj.c_AssignedDate = DateTime.Now;
            userObj.token = "";
            await _appDbContext.Customers.AddAsync(userObj);
            await _appDbContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "User Registered!"
            });

        }
        
        private Task<bool> checkUserNameExistAsync(string userName)
         => _appDbContext.Customers.AnyAsync(x => x.c_FullName == userName);
         private Task<bool> checkAddresExistAsync(string userAddres)
         => _appDbContext.Customers.AnyAsync(x => x.c_Address == userAddres);  
         private Task<bool> checkContactNumberExistAsync(string userContact)
         => _appDbContext.Customers.AnyAsync(x => x.c_ContactNo == userContact);  
         private Task<bool> checkNicExistAsync(string userNic)
         => _appDbContext.Customers.AnyAsync(x => x.c_Nic == userNic);

        //delete
        [HttpDelete("CustomersDelete{id:Guid}")]
        public async Task<IActionResult> CustomersDelete(Guid id)
        {
            var oCustomer = await _appDbContext.Customers.FindAsync(id);
            if (oCustomer != null)
            {
                _appDbContext.Customers.Remove(oCustomer);
                await _appDbContext.SaveChangesAsync();
                return Ok(oCustomer);
            }
            else
            {
                return NotFound();
            }
        }

        //Update
        [HttpPut("CustomerUpdate{id:Guid}")]
        public async Task<IActionResult> CustomerUpdate(Guid id, Customer oCustomers)
        {
            try
            {
                var cus = await _appDbContext.Customers.FirstOrDefaultAsync(x => x.id == oCustomers.id);
                if (cus != null)
                {
                    cus.c_FullName = oCustomers.c_FullName;
                    cus.c_Address = oCustomers.c_Address;
                    cus.c_ContactNo = oCustomers.c_ContactNo;
                    cus.c_Nic = oCustomers.c_Nic;
                    cus.modifiedUser = oCustomers.modifiedUser;
                    cus.c_AssignedDate = oCustomers.c_AssignedDate;
                    cus.token = oCustomers.token;
                    await _appDbContext.SaveChangesAsync();
                    return Ok(cus);
                }
                else
                {
                    return NotFound();

                }
            }
            catch (System.Exception)
            {
                return Ok();
                throw;
            }
        }

        //serch
        [HttpGet("serchFrists{id:Guid}")]
        public async Task<IActionResult> serchFrists([FromRoute] Guid id)
        {
            var frist = await _appDbContext.Customers.FirstOrDefaultAsync(x => x.id == id);

            if (frist == null)
            {
                return Ok(frist);
            }
            return Ok(frist);

        }





    }
}
