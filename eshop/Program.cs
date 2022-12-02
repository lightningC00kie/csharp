using NezarkaBookstore;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;
using System.IO;

class Top
{
    public static void Main(String[] args)
    {
        ModelStore? store = new ModelStore();
        List<String> requests = readInput();
        TextReader text_reader = File.OpenText("./in.txt");
        store = ModelStore.LoadFrom(text_reader);

        if (store != null)
        {
            foreach (String req in requests)
            {
                if (ProcessRequest.CheckRequestFormat(req))
                {
                    Request request = ProcessRequest.ParseReq(req);
                    ProcessRequest.FulfillRequest(store, request);
                }
                else
                {
                    HTMLTemplates.InvalidTemplate();
                }
                WriteLine("====");
            }
        }
        else
        {
            WriteLine("Data error.");
            return;
        }
    }

    static List<String> readInput()
    {
        String input = "";
        List<String> requests = new List<string>();
        bool is_req = false;
        String? line;
        while ((line = ReadLine()) != null)
        {
            if (line == "" && is_req)
            {
                continue;
            }
            if (is_req)
            {
                requests.Add(line);
                continue;
            }
            input += line + '\n';
            if (line == "DATA-END")
            {
                is_req = true;
            }
        }
        using (StreamWriter sw = new StreamWriter($"./in.txt"))
        {
            sw.Write(input);
        }
        return requests;
    }


}

class ProcessRequest
{
    public static bool CheckDataFormat(String data)
    {
        String[] dataArray = data.Split(";");
        List<String> types = new List<string>();
        types.Add("BOOK"); types.Add("CUSTOMER"); types.Add("CART-ITEM");
        if (!types.Contains(dataArray[0]))
        {
            return false;
        }

        if (dataArray[0] == "BOOK")
        {
            if (dataArray.Length != 5)
            {
                return false;
            }

            if (!int.TryParse(dataArray[1], out _) || !int.TryParse(dataArray[4], out _))
            {
                return false;
            }
        }

        else if (dataArray[0] == "CUSTOMER")
        {
            if (dataArray.Length != 4)
            {
                return false;
            }

            if (!int.TryParse(dataArray[1], out _))
            {
                return false;
            }
        }

        else if (dataArray[0] == "CART-ITEM")
        {
            if (dataArray.Length != 4)
            {
                return false;
            }

            if (!int.TryParse(dataArray[1], out _) || !int.TryParse(dataArray[2], out _) || !int.TryParse(dataArray[3], out _))
            {
                return false;
            }
        }
        return true;
    }

    public static bool CheckRequestFormat(string req)
    {
        List<String> split_list = new List<string>();
        String[] split_req;
        foreach (String s in req.Split(" "))
        {
            if (s != "")
            {
                split_list.Add(s);
            }
        }
        split_req = split_list.ToArray();
        if (split_req.Length != 3)
        {
            return false;
        }

        String[] split_url = split_req[2].Split("/");

        if (split_url.Length < 3)
        {
            return false;
        }

        if (split_req[0] != "GET" || !int.TryParse(split_req[1], out _))
        {
            return false;
        }

        if (!CheckURLFormat(split_url))
        {
            return false;
        }

        if (split_url.Length >= 5)
        {
            String action = split_url[4];
            String parentPage = split_url[3];
            if (!IsAction(action))
            {
                return false;
            }
            else if (split_url.Length == 6)
            {
                if (!int.TryParse(split_url[5], out _))
                {
                    return false;
                }
                if (action == "Add" || action == "Remove")
                {
                    if (parentPage != "ShoppingCart")
                    {
                        return false;
                    }
                }
                else if (action == "Detail")
                {
                    if (parentPage != "Books")
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private static bool CheckURLFormat(string[] url)
    {
        return url[0] == "http:" && url[2] == "www.nezarka.net" && (url[3] == "Books" || url[3] == "ShoppingCart");
    }

    public static Request ParseReq(String to_parse)
    {
        int bookId;
        String? action = null;
        List<String> split_list = new List<string>();
        String[] split_req;
        foreach (String s in to_parse.Split(" "))
        {
            if (s != "")
            {
                split_list.Add(s);
            }
        }
        split_req = split_list.ToArray();
        String[] split_url = split_req[2].Split("/");

        if (IsAction(split_url[split_url.Length - 2]))
        {
            action = split_url[split_url.Length - 2];
        }

        bool isNumeric = int.TryParse(split_url[split_url.Length - 1], out bookId);
        string parentPage = isNumeric ? split_url[split_url.Length - 3] : split_url[split_url.Length - 1];

        Request req = new Request(int.Parse(split_req[1]), //custId
        parentPage, //parentPage
        bookId == 0 ? null : bookId, //bookId
        action //action
        );
        return req;
    }

    public static bool RequestIsValid(ModelStore store, Request req)
    {

        if (!CustomerExists(store, req.custId))
        {
            return false;
        }

        var customer = store.GetCustomer(req.custId);

        if (req.bookId != null)
        {
            if (!BookExists(store, (int)req.bookId))
            {
                return false;
            }
            if (req.action == "Remove")
            {
                if (!BookInCart(store, req))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static bool CustomerExists(ModelStore store, int CustId)
    {
        return store.GetCustomer(CustId) == null ? false : true;
    }

    private static bool BookExists(ModelStore store, int BookId)
    {
        return store.GetBook(BookId) == null ? false : true;
    }

    private static bool BookInCart(ModelStore store, Request req)
    {
        var customer = store.GetCustomer(req.custId);
        foreach (ShoppingCartItem i in customer!.ShoppingCart.Items)
        {
            if (i.BookId == req.bookId)
            {
                return true;
            }
        }
        return false;
    }

    public static void FulfillRequest(ModelStore store, Request req)
    {
        if (!RequestIsValid(store, req))
        {
            HTMLTemplates.InvalidTemplate();
            return;
        }
        var customer = store.GetCustomer(req.custId);
        var items = customer!.ShoppingCart.Items;
        if (req.parentPage == "Books")
        {
            if (!IsAction(req.action))
            {
                IList<Book> books = store.GetBooks();
                HTMLTemplates.GetBooksTemplate(store, req, books);
            }
            else if (IsAction(req.action))
            {
                if (req.action == "Detail")
                {
                    HTMLTemplates.GetBookDetailTemplate(store, req);
                }
            }
        }
        else if (req.parentPage == "ShoppingCart")
        {
            if (!IsAction(req.action))
            {
                if (customer.ShoppingCart.ShoppingCartCount() == 0)
                {
                    HTMLTemplates.EmptyCartTemplate(store, req);
                }
                else
                {
                    HTMLTemplates.GetCartTemplate(store, req);
                }
            }
            else
            {
                if (req.action == "Add")
                {
                    AddBook(items, (int)req.bookId!);
                    HTMLTemplates.GetCartTemplate(store, req);
                }
                else if (req.action == "Remove")
                {
                    RemoveBook(store, req, items, (int)req.bookId!);
                    if (items.Count > 0)
                    {
                        HTMLTemplates.GetCartTemplate(store, req);
                    }
                    else
                    {
                        HTMLTemplates.EmptyCartTemplate(store, req);
                    }

                }
            }
        }
    }

    public static void AddBook(List<ShoppingCartItem> items, int bookId)
    {
        bool foundBook = false;
        foreach (ShoppingCartItem i in items)
        {
            if (i.BookId == bookId)
            {
                foundBook = true;
                i.Count++;
            }
        }
        if (!foundBook)
        {
            ShoppingCartItem book = new ShoppingCartItem();
            book.BookId = bookId;
            book.Count = 1;
            items.Add(book);
        }
    }

    public static void RemoveBook(ModelStore store, Request req, List<ShoppingCartItem> items, int bookId)
    {
        bool removeEntry = false;
        ShoppingCartItem? toBeRemoved = null;
        foreach (ShoppingCartItem i in items)
        {

            if (i.BookId == bookId)
            {
                if (i.Count > 0)
                {
                    i.Count--;
                    if (i.Count == 0)
                    {
                        removeEntry = true;
                        toBeRemoved = i;
                    }
                }
                else
                {
                    HTMLTemplates.InvalidTemplate();
                }
            }
        }

        if (removeEntry)
        {
            items.Remove(toBeRemoved!);
        }
    }

    private static bool IsAction(String? action)
    {
        return action == "Add" || action == "Remove" || action == "Detail";
    }
}

class Request
{
    public int custId;
    public int? bookId;
    public String? action; // action can be one of three things: add, remove, detail
    public String parentPage; // parentPage can be either Books or ShoppingCart
    public Request(int custId, String parentPage, int? bookId, String? action)
    {
        this.custId = custId; this.bookId = bookId; this.action = action; this.parentPage = parentPage;
    }
}

class HTMLTemplates
{
    public static void HeaderTemplate(ModelStore store, Request req)
    {
        var customer = store.GetCustomer(req.custId);
        WriteLine($"<!DOCTYPE html>");
        WriteLine($"<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
        WriteLine($"<head>");
        WriteLine($"	<meta charset=\"utf-8\" />");
        WriteLine($"	<title>Nezarka.net: Online Shopping for Books</title>");
        WriteLine($"</head>");
        WriteLine($"<body>");
        WriteLine($"	<style type=\"text/css\">");
        WriteLine("		table, th, td {");
        WriteLine($"			border: 1px solid black;");
        WriteLine($"			border-collapse: collapse;");
        WriteLine("		}");
        WriteLine("		table {");
        WriteLine($"			margin-bottom: 10px;");
        WriteLine("		}");
        WriteLine("		pre {");
        WriteLine($"			line-height: 70%;");
        WriteLine("		}");
        WriteLine($"	</style>");
        WriteLine($"	<h1><pre>  v,<br />Nezarka.NET: Online Shopping for Books</pre></h1>");
        WriteLine($"	{customer!.FirstName}, here is your menu:");
        WriteLine($"	<table>");
        WriteLine($"		<tr>");
        WriteLine($"			<td><a href=\"/Books\">Books</a></td>");
        WriteLine($"			<td><a href=\"/ShoppingCart\">Cart ({customer.ShoppingCart.ShoppingCartCount()})</a></td>");
        WriteLine($"		</tr>");
        WriteLine($"	</table>");
    }

    public static void GetBooksTemplate(ModelStore store, Request req, IList<Book> books)
    {
        var customer = store.GetCustomer(req.custId);
        HeaderTemplate(store, req);
        WriteLine($"	Our books for you:");
        WriteLine($"	<table>");
        for (int i = 0; i < Math.Ceiling((double)books.Count / 3); i++)
        {
            WriteLine($"		<tr>");
            for (int j = 0; j < 3; j++)
            {
                int index = (i * 3) + j;
                if (index < books.Count)
                {
                    WriteLine($"			<td style=\"padding: 10px;\">");
                    WriteLine($"				<a href=\"/Books/Detail/{books[index].Id}\">{books[index].Title}</a><br />");
                    WriteLine($"				Author: {books[index].Author}<br />");
                    WriteLine($"				Price: {books[index].Price} EUR &lt;<a href=\"/ShoppingCart/Add/{books[index].Id}\">Buy</a>&gt;");
                    WriteLine($"			</td>");
                }
            }
            WriteLine($"		</tr>");
        }
        WriteLine($"	</table>");
        WriteLine($"</body>");
        WriteLine($"</html>");
    }

    public static void GetBookDetailTemplate(ModelStore store, Request req)
    {
        var customer = store.GetCustomer(req.custId);
        var book = store.GetBook((int)req.bookId!);
        HeaderTemplate(store, req);
        WriteLine($"	Book details:");
        WriteLine($"	<h2>{book!.Title}</h2>");
        WriteLine($"	<p style=\"margin-left: 20px\">");
        WriteLine($"	Author: {book.Author}<br />");
        WriteLine($"	Price: {book.Price} EUR<br />");
        WriteLine($"	</p>");
        WriteLine($"	<h3>&lt;<a href=\"/ShoppingCart/Add/{book.Id}\">Buy this book</a>&gt;</h3>");
        WriteLine($"</body>");
        WriteLine($"</html>");
    }

    public static void GetCartTemplate(ModelStore store, Request req)
    {
        var customer = store.GetCustomer(req.custId);
        int numBooks = customer!.ShoppingCart.Items.Count;
        int totalPrice = 0;
        HeaderTemplate(store, req);
        WriteLine($"	Your shopping cart:");
        WriteLine($"	<table>");
        WriteLine($"		<tr>");
        WriteLine($"			<th>Title</th>");
        WriteLine($"			<th>Count</th>");
        WriteLine($"			<th>Price</th>");
        WriteLine($"			<th>Actions</th>");
        WriteLine($"		</tr>");
        for (int i = 0; i < numBooks; i++)
        {
            ShoppingCartItem item = customer.ShoppingCart.Items[i];
            Book book = store.GetBook(item.BookId)!;
            if (item.Count > 0)
            {
                WriteLine($"		<tr>");
                WriteLine($"			<td><a href=\"/Books/Detail/{book.Id}\">{book.Title}</a></td>");
                WriteLine($"			<td>{item.Count}</td>");
                if (item.Count > 1)
                {
                    WriteLine($"			<td>{item.Count} * {book.Price} = {item.Count * book.Price} EUR</td>");
                }
                else
                {
                    WriteLine($"			<td>{book.Price} EUR</td>");
                }
                WriteLine($"			<td>&lt;<a href=\"/ShoppingCart/Remove/{book.Id}\">Remove</a>&gt;</td>");
                WriteLine($"		</tr>");
            }
            else
            {
                continue;
            }
            totalPrice += item.Count * (int)book.Price;
        }
        WriteLine($"	</table>");
        WriteLine($"	Total price of all items: {totalPrice} EUR");
        WriteLine($"</body>");
        WriteLine($"</html>");
    }

    public static void EmptyCartTemplate(ModelStore store, Request req)
    {
        var customer = store.GetCustomer(req.custId);
        HeaderTemplate(store, req);
        WriteLine($"	Your shopping cart is EMPTY.");
        WriteLine($"</body>");
        WriteLine($"</html>");
    }

    public static void InvalidTemplate()
    {
        WriteLine($"<!DOCTYPE html>");
        WriteLine($"<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
        WriteLine($"<head>");
        WriteLine($"	<meta charset=\"utf-8\" />");
        WriteLine($"	<title>Nezarka.net: Online Shopping for Books</title>");
        WriteLine($"</head>");
        WriteLine($"<body>");
        WriteLine($"<p>Invalid request.</p>");
        WriteLine($"</body>");
        WriteLine($"</html>");
    }
}