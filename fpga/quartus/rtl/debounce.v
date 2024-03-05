/*
 * Implements a button debouncer with a debounce time of 10ms
 * 50000 / 50 MHz  = 10ms
 *
 * Guide : https://zipcpu.com/blog/2017/08/04/debouncing.html
 *
*/

module debounce(
	//////////// INPUTS //////////
	input wire	clk,
	input wire	in,
	
	//////////// OUTPUT //////////
	output reg	out
);

//=======================================================
//  REG/WIRE declarations
//=======================================================

	parameter BITS = 16;
	
	reg						different;
	reg 						ztimer;
	reg						last;
	reg	[(BITS-1):0]	timer;
	
	// ztimer holds whether or not the clock is stopped (timer == 0).
	// More efficient to implement rather than (timer == 0) everytime
	initial	ztimer 		= 1'b1;
	initial	timer  		= 0;
	initial	different 	= 0;
	initial	out 			= 1'b0;
	
	
//=======================================================
//  Structural coding
//=======================================================	

	// Keep track of the last input
	always @(posedge clk)
		last <= in;

	// Start a timer any time there is a change in inputs.
	// When the timer finishes, check if things have changed
	// and start over immediately if so.  
	always @(posedge clk)
		if ((ztimer) && (different)) begin
			timer  <= 16'd50000;
			ztimer <= 1'b0;
		end 
		else if (!ztimer) begin
			timer  <= timer - 1'b1;
			// We ignore the last bit as this is 1-bit delayed
			ztimer <= (timer[(BITS-1):1] == 0);
		end 
		else begin
			timer  <= 0;
			ztimer <= 1'b1;
		end

	// Keep track of whether or not the timer needs to be restarted.
	// different will get set to "true" when 
	// 1. input != output - a button has just been registered
	// OR
	// 2. A change has been registered whilst the timer is running
	// and needs to be kept true to not miss an input
	always @(posedge clk)
		different <= ((different) && (!ztimer)) || (in != out);

	// Output = input only when the timer is NOT running
	always @(posedge clk)
		if (ztimer)
			out <= last;

endmodule
