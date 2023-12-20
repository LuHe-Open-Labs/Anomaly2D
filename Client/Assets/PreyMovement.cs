using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PreyMovement : MonoBehaviour
{
  List<List<List<List<float>>>> layers = new List<List<List<List<float>>>>();
  List<List<List<float>>> firstLayer = new List<List<List<float>>>();
  List<List<float>> tempFirstLayer = new List<List<float>>();
  List<List<List<float>>> layer = new List<List<List<float>>>();
  private int layerIndex = 0;
  List<List<float>> neuron = new List<List<float>>();
  List<string> neuronStrings = new List<string>();

  private float sum;


  public float maxDistance = 70f;
  public float blobEnergy = 100f;
  public int blobCopy = 0;

  public bool AiControlled = true;

  public float playerMovementSpeed = 1f;
  public float playerTurningSpeed = 5f;

  public float mapSize = 160f;

  public float actualAngle = 0f;

  private Coroutine updateVisionCoroutine;
  private Coroutine saveNeuralNetwork;

  private void moveBlob(int movementDirection)
  {
    if (movementDirection == 1)
    {
      transform.position += new Vector3(-Mathf.Sin(actualAngle * Mathf.Deg2Rad) * playerMovementSpeed, Mathf.Cos(actualAngle * Mathf.Deg2Rad) * playerMovementSpeed, 0);
    }
    if (movementDirection == -1)
    {
      transform.position += new Vector3(Mathf.Sin(actualAngle * Mathf.Deg2Rad) * playerMovementSpeed, -Mathf.Cos(actualAngle * Mathf.Deg2Rad) * playerMovementSpeed, 0);
    }
    if (movementDirection == 2)
    {
      transform.Rotate(0, 0, playerTurningSpeed);
      if (actualAngle + playerTurningSpeed > 360)
      {
        actualAngle = 0f + playerTurningSpeed;
      }
      else
      {
        actualAngle += playerTurningSpeed;
      }
    }
    if (movementDirection == -2)
    {
      transform.Rotate(0, 0, -playerTurningSpeed);
      if (actualAngle - playerTurningSpeed < 0)
      {
        actualAngle = 360f - playerTurningSpeed;
      }
      else
      {
        actualAngle -= playerTurningSpeed;
      }
    }
  }

  void Raycast(float angle, int rayIndex)
  {
    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0), maxDistance);

    if (hits.Length == 1)
    {
      layers[0][rayIndex][0][0] = 0;
    }

    for (int i = 1; i < hits.Length; i++)
    {
      RaycastHit2D hit = hits[i];
      if (hit.collider != null && hit.collider.gameObject != null && hit.collider.gameObject != gameObject)
      {
        if (hit.collider.gameObject.tag == "Prey")
        {
          //continue;
          layers[0][rayIndex][0][0] = 1;
          //Debug.DrawRay(transform.position, new Vector3(hit.point.x - transform.position.x, hit.point.y - transform.position.y, transform.position.z), Color.green);
          break; // Break out of the loop after the first valid hit
        }
        else
        {
          layers[0][rayIndex][0][0] = -1;
          //Debug.DrawRay(transform.position, new Vector3(hit.point.x - transform.position.x, hit.point.y - transform.position.y, transform.position.z), Color.blue);
          break; // Break out of the loop after the first valid hit
        }
      }
    }
  }

  float LogSigmoid(float input)
  {
    if (input < -45.0f) return -1.0f;
    else if (input > 45.0f) return 1.0f;
    else return ((1.0f / (1.0f + Mathf.Exp(-input))) - 0.5f) * 2;
  }

  void Calculate()
  {
    if (AiControlled)
    {
      for (int layerId = 1; layerId < 4; layerId++)
      {
        for (int neuronId = 0; neuronId < layers[layerId].Count; neuronId++)
        {
          sum = 0f;
          for (int i = 0; i < layers[layerId - 1].Count - 1; i++)
          {
            sum = sum + (layers[layerId - 1][i][layers[layerId - 1][i].Count - 1][0] * layers[layerId][neuronId][i][0]);
          }
          layers[layerId][neuronId][layers[layerId][neuronId].Count - 1][0] = LogSigmoid(sum + 0.5f);

        }
      }

      if (layers[3][0][10][0] > 0)
      {
        moveBlob(1);
        blobEnergy -= 2.0f;
      }
      if (layers[3][1][10][0] > 0)
      {
        //moveBlob(-1);
      }
      if (layers[3][2][10][0] > 0)
      {
        moveBlob(-2);
        blobEnergy -= 0.75f;
      }
      if (layers[3][3][10][0] > 0)
      {
        moveBlob(2);
        blobEnergy -= 0.75f;
      }
    }
  }

  void Mutate()
  {
    for (int layIndex = 1; layIndex < 4; layIndex++)
    {
      for (int neurIndex = 0; neurIndex < layers[layIndex].Count - 1; neurIndex++)
      {
        for (int weightIndex = 0; weightIndex < layers[layIndex][neurIndex].Count - 2; weightIndex++)
        {
          if (Random.Range(0, 100) <= 2.0f)
          {
            layers[layIndex][neurIndex][weightIndex][0] = Random.Range(-1.0f, 1.0f);
          }
        }
      }
    }

    //List<string> layerStrings = ntransform.position = new Vector3(transform.position.x+1f, transform.position.y, transform.position.z);ew List<string>();
    //foreach (List<List<List<float>>> layerString in layers)
    //{
    //    List<string> neuronStrings = new List<string>();
    //    foreach (List<List<float>> neuronString in layerString)
    //    {
    //        List<string> sublistStrings = new List<string>();
    //        foreach (List<float> sublist in neuronString)
    //        {
    //            sublistStrings.Add("{" + string.Join(",", sublist) + "}");
    //        }
    //        neuronStrings.Add("{" + string.Join(",", sublistStrings) + "}");
    //    }
    //    layerStrings.Add("{" + string.Join(",", neuronStrings) + "}");
    //}
    //Debug.Log("{" + string.Join(",", layerStrings) + "}"); // output the entire list as a string
  }

  // Start is called before the first frame update
  void Start()
  {
    layers.Add(new List<List<List<float>>>(){
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            },
            new List<List<float>>(){
                new List<float>() {0}
            }
        });
    for (int i = 0; i < 3; i++)
    {
      layers.Add(new List<List<List<float>>>());
    }
    foreach (List<List<List<float>>> layer in layers)
    {
      if (layerIndex == 0)
      {
        //Debug.Log("Shitty first layer");
      }
      else
      {
        if (layerIndex != 3)
        {
          for (int i = 0; i < 10; i++)
          {
            layer.Add(new List<List<float>>());
          }
        }
        else
        {
          for (int i = 0; i < 4; i++)
          {
            layer.Add(new List<List<float>>());
          }
        }


        foreach (List<List<float>> neuron in layer)
        {
          if (layerIndex == 1)
          {
            for (int i = 0; i < 18; i++)
            {
              neuron.Add(new List<float>() { Random.Range(-1.0f, 1.0f) });
            }
            neuron.Add(new List<float>() { 0f });
          }
          if (layerIndex == 2 || layerIndex == 3)
          {
            for (int i = 0; i < 10; i++)
            {
              neuron.Add(new List<float>() { Random.Range(-1.0f, 1.0f) });
            }
            neuron.Add(new List<float>() { 0f });
          }
        }
      }
      layerIndex++;
    }

    //List<string> layerStrings = new List<string>();
    //foreach (List<List<List<float>>> layerString in layers)
    //{
    //    List<string> neuronStrings = new List<string>();
    //    foreach (List<List<float>> neuronString in layerString)
    //    {
    //        List<string> sublistStrings = new List<string>();
    //        foreach (List<float> sublist in neuronString)
    //        {
    //            sublistStrings.Add("{" + string.Join(",", sublist) + "}");
    //        }
    //        neuronStrings.Add("{" + string.Join(",", sublistStrings) + "}");
    //    }
    //    layerStrings.Add("{" + string.Join(",", neuronStrings) + "}");
    //}
    //Debug.Log("{" + string.Join(",", layerStrings) + "}"); // output the entire list as a string


    //----------DEBUG SHIT, DO NOT TOUCH----------//


    //Test(new List<float>() {1, 2, 3, 4, 5, 6, 7});

    //value = neurons[0][1];
    //foreach (List<float> neuronString in layers[2][0])
    //{
    //    neuronStrings.Add(string.Join(",", neuronString));
    //}
    //Debug.Log("{" + string.Join("}, {", neuronStrings) + "}"); // output the entire list as a string

    //Debug.Log(string.Join(",", neurons[0])); // output the first list as a string
    //Debug.Log(value);


    //----------BACK TO REAL CODE----------//


    Application.targetFrameRate = 30; // set the target frame rate to 30 fps
    Mutate();

    GameObject originalObject = gameObject;
    if (gameObject.name == "Prey")
    {
      for (int i = 0; i < 150; i++)
      {
        GameObject duplicatedObject = Instantiate(originalObject);

        duplicatedObject.transform.position = new Vector3(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize), transform.position.z);

        PreyMovement preyMovement = duplicatedObject.GetComponent<PreyMovement>();
        preyMovement.blobEnergy = 100f;
        preyMovement.AiControlled = true;

        blobEnergy = 100f;
      }
    }

    updateVisionCoroutine = StartCoroutine(UpdateBlobVision());
    saveNeuralNetwork = StartCoroutine(Save());
  }

  private IEnumerator Save()
  {
    while (true)
    {
      yield return new WaitForSecondsRealtime(300);

      List<string> layerStrings = new List<string>();
      foreach (List<List<List<float>>> layerString in layers)
      {
        List<string> neuronStrings = new List<string>();
        foreach (List<List<float>> neuronString in layerString)
        {
          List<string> sublistStrings = new List<string>();
          foreach (List<float> sublist in neuronString)
          {
            sublistStrings.Add("{" + string.Join(",", sublist) + "}");
          }
          neuronStrings.Add("{" + string.Join(",", sublistStrings) + "}");
        }
        layerStrings.Add("{" + string.Join(",", neuronStrings) + "}");
      }
      Debug.Log("{" + string.Join(",", layerStrings) + "}"); // output the entire list as a string

      using (StreamWriter writer = new StreamWriter("C:/Users/hugol/OneDrive/Bureau/Cours 2022/NSI/Unity/Anomaly/Assets/NeuralNetwork.txt"))
      {
        Debug.Log("YSK2");
        writer.Write("{" + string.Join(",", layerStrings) + "}");
      }
    }

  }

  private IEnumerator UpdateBlobVision()
  {
    while (true)
    {
      for (int i = 0; i < 18; i++)
      {
        yield return new WaitForSeconds(0.025f);
        Raycast((i * 20) + actualAngle, i);
        Calculate();

        if (blobEnergy >= 200.0f)
        {
          if (blobCopy < 2)
          {
            GameObject originalObject = gameObject;
            GameObject duplicatedObject = Instantiate(originalObject);

            duplicatedObject.transform.position = new Vector3(transform.position.x + Random.Range(-5.0f, 5.00001f), transform.position.y + Random.Range(-5.0f, 5.00001f), transform.position.z);

            PreyMovement preyMovement = duplicatedObject.GetComponent<PreyMovement>();
            preyMovement.blobEnergy = 100f;
            preyMovement.AiControlled = true;
            preyMovement.blobCopy = Random.Range(0, 1);
            yield return new WaitForSeconds(0.001f);
            preyMovement.layers = layers;

            blobEnergy = 100f;
            blobCopy += 1;
          }
          else
          {
            GameObject originalObject = gameObject;
            Destroy(gameObject);
          }

        }
        else if (blobEnergy <= 0f)
        {
          GameObject originalObject = gameObject;
          Destroy(gameObject);
        }
        else
        {
          blobEnergy += 2f;
        }

        if (Mathf.Abs(transform.position.x) > mapSize)
        {
          transform.position = new Vector3(Mathf.Sign(transform.position.x) * -(mapSize - 5f), transform.position.y, transform.position.z);
        }
        if (Mathf.Abs(transform.position.y) > mapSize)
        {
          transform.position = new Vector3(transform.position.x, Mathf.Sign(transform.position.y) * -(mapSize - 5f), transform.position.z);
        }
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (!AiControlled)
    {
      if (Input.GetKey(KeyCode.Z))
      {
        moveBlob(1);
      }
      if (Input.GetKey(KeyCode.S))
      {
        moveBlob(-1);
      }
      if (Input.GetKey(KeyCode.Q))
      {
        moveBlob(2);
      }
      if (Input.GetKey(KeyCode.D))
      {
        moveBlob(-2);
      }
    }
    else
    {

    }
  }
}